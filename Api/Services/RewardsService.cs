using GpsUtil.Location;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;

namespace TourGuide.Services;

public class RewardsService : IRewardsService
{
    private const double StatuteMilesPerNauticalMile = 1.15077945;
    private readonly int _defaultProximityBuffer = 10;
    private int _proximityBuffer;
    private readonly int _attractionProximityRange = 200;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardCentral _rewardsCentral;
    private static int count = 0;

    public RewardsService(IGpsUtil gpsUtil, IRewardCentral rewardCentral)
    {
        _gpsUtil = gpsUtil;
        _rewardsCentral =rewardCentral;
        _proximityBuffer = _defaultProximityBuffer;
    }

    public async Task SetProximityBuffer(int proximityBuffer)
    {
        _proximityBuffer = proximityBuffer;
        await Task.CompletedTask;
    }

    public async Task SetDefaultProximityBuffer()
    {
        _proximityBuffer = _defaultProximityBuffer; 
        await Task.CompletedTask; 
    }

    public async Task CalculateRewards(User user) 
    {
        count++;
        List<VisitedLocation> userLocations = user.VisitedLocations.ToList();
        List<Attraction> attractions = await _gpsUtil.GetAttractions();

        var rewardsToAdd = new List<UserReward>();

        foreach (var visitedLocation in userLocations)
        {
            foreach (var attraction in attractions)
            {
                bool alreadyRewarded = user.UserRewards
                    .Any(r => r.Attraction.AttractionName == attraction.AttractionName);

                if (!alreadyRewarded && await NearAttraction(visitedLocation, attraction))
                {
                    var reward = new UserReward(
                        visitedLocation,
                        attraction,
                        await GetRewardPoints(attraction, user)

                        );
                    rewardsToAdd.Add(reward);
                }

                //if (!user.UserRewards.Any(r => r.Attraction.AttractionName == attraction.AttractionName))
                //{
                //    if (NearAttraction(visitedLocation, attraction))
                //    {
                //        user.AddUserReward(new UserReward(visitedLocation, attraction, GetRewardPoints(attraction, user)));
                //    }
                //}
            }

            // On ajoute les récompences apres la boucle pour eviter l'erreur
            foreach(var reward in rewardsToAdd)
            {
                user.AddUserReward(reward); 
            }
        }
    }

    public async Task<bool> IsWithinAttractionProximity(Attraction attraction, Locations location)
    {
        Console.WriteLine(GetDistance(attraction, location));
        return await GetDistance(attraction, location) <= _attractionProximityRange;
    }

    private async Task<bool> NearAttraction(VisitedLocation visitedLocation, Attraction attraction)
    {
        return await GetDistance(attraction, visitedLocation.Location) <= _proximityBuffer;
    }

    public async Task<int> GetRewardPoints(Attraction attraction, User user)
    {
        return await _rewardsCentral.GetAttractionRewardPoints(attraction.AttractionId, user.UserId);
    }

    public async Task<double> GetDistance(Locations loc1, Locations loc2)
    {
        double lat1 = Math.PI * loc1.Latitude / 180.0;
        double lon1 = Math.PI * loc1.Longitude / 180.0;
        double lat2 = Math.PI * loc2.Latitude / 180.0;
        double lon2 = Math.PI * loc2.Longitude / 180.0;

        double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2)
                                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2));

        double nauticalMiles = 60.0 * angle * 180.0 / Math.PI;
        return await Task.FromResult(StatuteMilesPerNauticalMile * nauticalMiles);
    }
}
