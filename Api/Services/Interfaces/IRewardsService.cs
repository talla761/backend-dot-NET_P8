using GpsUtil.Location;
using TourGuide.Users;

namespace TourGuide.Services.Interfaces
{
    public interface IRewardsService
    {
        Task CalculateRewards(User user);
        Task<double> GetDistance(Locations loc1, Locations loc2);
        Task<bool> IsWithinAttractionProximity(Attraction attraction, Locations location);
        Task SetDefaultProximityBuffer();
        Task SetProximityBuffer(int proximityBuffer);
        Task<int> GetRewardPoints(Attraction attraction, User user); 

    }
}