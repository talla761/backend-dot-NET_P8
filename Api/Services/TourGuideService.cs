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

    public async Task<List<UserReward>> GetUserRewards(User user)
    {
        return user.UserRewards;
    }

    public async Task<VisitedLocation> GetUserLocation(User user)
    {
        return user.VisitedLocations.Any() 
            ? user.GetLastVisitedLocation() 
            : await TrackUserLocation(user);
    }

    public async Task<User> GetUser(string userName)
    {
        return _internalUserMap.ContainsKey(userName) ? _internalUserMap[userName] : null;
    }

    public async Task<List<User>> GetAllUsers()
    {
        return _internalUserMap.Values.ToList();
    }

    public Task AddUser(User user)
    {
        if (!_internalUserMap.ContainsKey(user.UserName))
        {
            _internalUserMap.Add(user.UserName, user);
        }

        return Task.CompletedTask;
    }

    public async Task<List<Provider>> GetTripDeals(User user)
    {
        int cumulativeRewardPoints = user.UserRewards.Sum(i => i.RewardPoints);
        List<Provider> providers = await _tripPricer.GetPrice(TripPricerApiKey, 
            user.UserId,
            user.UserPreferences.NumberOfAdults, 
            user.UserPreferences.NumberOfChildren,
            user.UserPreferences.TripDuration, 
            cumulativeRewardPoints);

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

    public async Task<List<NearbyAttractionDto>> GetNearByAttractions(VisitedLocation visitedLocation, User user)
    {
        var attractions = await _gpsUtil.GetAttractions();

        // Récupérer toutes les distances et trier après
        var attractionsWithDistance = new List<(Attraction Attraction, double Distance)>();

        foreach (var attraction in attractions)
        {
            var distance = await _rewardsService.GetDistance(visitedLocation.Location, new Locations(attraction.Latitude, attraction.Longitude));
            attractionsWithDistance.Add((attraction, distance));
        }

        var sortedAttractions = attractionsWithDistance
            .OrderBy(x => x.Distance)
            .Take(5)
            .ToList();

        var result = new List<NearbyAttractionDto>();

        // Ajouter les résultats dans le DTO
        foreach (var item in sortedAttractions)
        {
            var rewardPoints = await _rewardsService.GetRewardPoints(item.Attraction, user);

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
