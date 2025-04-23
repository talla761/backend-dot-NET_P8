using GpsUtil.Location;
using TourGuide.Dto;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services.Interfaces
{
    public interface ITourGuideService
    {
        Tracker Tracker { get; }

        void AddUser(User user);
        List<User> GetAllUsers();
        Task<List<NearbyAttractionDto>> GetNearByAttractions(VisitedLocation visitedLocation, User user);
        Task<List<Provider>> GetTripDeals(User user);
        User GetUser(string userName);
        Task<VisitedLocation> GetUserLocation(User user);
        List<UserReward> GetUserRewards(User user);
        Task<VisitedLocation> TrackUserLocation(User user);
    }
}