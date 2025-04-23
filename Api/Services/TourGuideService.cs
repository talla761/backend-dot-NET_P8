using GpsUtil.Location;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using TourGuide.Dto;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services;

public class TourGuideService : ITourGuideService
{
    private readonly ILogger _logger;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardsService _rewardsService;
    private readonly TripPricer.TripPricer _tripPricer;
    public Tracker Tracker { get; private set; }
    private readonly Dictionary<string, User> _internalUserMap = new();
    private const string TripPricerApiKey = "test-server-api-key";
    private bool _testMode = true;

    public TourGuideService(ILogger<TourGuideService> logger, IGpsUtil gpsUtil, IRewardsService rewardsService, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _tripPricer = new();
        _gpsUtil = gpsUtil;
        _rewardsService = rewardsService;

        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        if (_testMode)
        {
            _logger.LogInformation("TestMode enabled");
            _logger.LogDebug("Initializing users");
            InitializeInternalUsers();
            _logger.LogDebug("Finished initializing users");
        }

        var trackerLogger = loggerFactory.CreateLogger<Tracker>();

        Tracker = new Tracker(this, trackerLogger);
        AddShutDownHook();
    }

    public List<UserReward> GetUserRewards(User user)
    {
        return user.UserRewards;
    }

    public async Task<VisitedLocation> GetUserLocation(User user)
    {
        return user.VisitedLocations.Any() 
            ? user.GetLastVisitedLocation() 
            : await TrackUserLocation(user);
    }

    public User GetUser(string userName)
    {
        return _internalUserMap.ContainsKey(userName) ? _internalUserMap[userName] : null;
    }

    public List<User> GetAllUsers()
    {
        return _internalUserMap.Values.ToList();
    }

    public void AddUser(User user)
    {
        if (!_internalUserMap.ContainsKey(user.UserName))
        {
            _internalUserMap.Add(user.UserName, user);
        }
    }

    public async Task<List<Provider>> GetTripDeals(User user)
    {
        int cumulativeRewardPoints = user.UserRewards.Sum(i => i.RewardPoints);
        List<Provider> providers = await Task.Run(() =>_tripPricer.GetPrice(TripPricerApiKey, 
            user.UserId,
            user.UserPreferences.NumberOfAdults, 
            user.UserPreferences.NumberOfChildren,
            user.UserPreferences.TripDuration, 
            cumulativeRewardPoints));

        user.TripDeals = providers;
        return providers;
    }

    public async Task<VisitedLocation> TrackUserLocation(User user)
    {
        VisitedLocation visitedLocation = await _gpsUtil.GetUserLocation(user.UserId);
        user.AddToVisitedLocations(visitedLocation);
        await _rewardsService.CalculateRewards(user);
        return visitedLocation;
    }

    //public async Task<List<Attraction>> GetNearByAttractions(VisitedLocation visitedLocation)
    //{
    //    List<Attraction> nearbyAttractions = new ();
    //    var attractions = await _gpsUtil.GetAttractions();

    //    foreach (var attraction in attractions)
    //    {
    //        if (_rewardsService.IsWithinAttractionProximity(attraction, visitedLocation.Location))
    //        {
    //            nearbyAttractions.Add(attraction);
    //        }
    //    }

    //    return nearbyAttractions;
    //}

    public async Task<List<NearbyAttractionDto>> GetNearByAttractions(VisitedLocation visitedLocation, User user)
    {
        var attractions = await _gpsUtil.GetAttractions();

        var sortedAttractions = attractions
            .Select(a => new
            {
                Attraction = a,
                Distance = _rewardsService.GetDistance(visitedLocation.Location, new Locations(a.Latitude, a.Longitude))
            })
            .OrderBy(x => x.Distance)
            .Take(5)
            .ToList();

        var result = new List<NearbyAttractionDto>();

        foreach (var item in sortedAttractions)
        {
            var rewardPoints = _rewardsService.GetRewardPoints(item.Attraction, user);

            result.Add(new NearbyAttractionDto
            {
                AttractionName = item.Attraction.AttractionName,
                AttractionLocation = new Locations(item.Attraction.Latitude, item.Attraction.Longitude),
                UserLocation = visitedLocation.Location,
                DistanceInMiles = item.Distance,
                RewardPoints = rewardPoints
            });
        }

        return result;
    }


    private void AddShutDownHook()
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => Tracker.StopTracking();
    }

    /**********************************************************************************
    * 
    * Methods Below: For Internal Testing
    * 
    **********************************************************************************/

    private void InitializeInternalUsers()
    {
        for (int i = 0; i < InternalTestHelper.GetInternalUserNumber(); i++)
        {
            var userName = $"internalUser{i}";
            var user = new User(Guid.NewGuid(), userName, "000", $"{userName}@tourGuide.com");
            GenerateUserLocationHistory(user);
            _internalUserMap.Add(userName, user);
        }

        _logger.LogDebug($"Created {InternalTestHelper.GetInternalUserNumber()} internal test users.");
    }

    private void GenerateUserLocationHistory(User user)
    {
        for (int i = 0; i < 3; i++)
        {
            var visitedLocation = new VisitedLocation(user.UserId, new Locations(GenerateRandomLatitude(), GenerateRandomLongitude()), GetRandomTime());
            user.AddToVisitedLocations(visitedLocation);
        }
    }

    private static readonly Random random = new Random();

    private double GenerateRandomLongitude() 
    {
        //return new Random().NextDouble() * (180 - (-180)) + (-180);
        return random.NextDouble() * (180 - (-180)) + (-180);
    }

private double GenerateRandomLatitude()
    {
        //return new Random().NextDouble() * (90 - (-90)) + (-90);
        return random.NextDouble() * (90 - (-90)) + (-90);
    }

    private DateTime GetRandomTime()
    {
        return DateTime.UtcNow.AddDays(-new Random().Next(30));
    }
}
