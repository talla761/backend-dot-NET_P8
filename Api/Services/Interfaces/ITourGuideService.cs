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

        Task AddUser(User user);
        Task<List<User>> GetAllUsers();
        Task<List<NearbyAttractionDto>> GetNearByAttractions(VisitedLocation visitedLocation, User user);
        Task<List<Provider>> GetTripDeals(User user);
        Task<User> GetUser(string userName);
        Task<VisitedLocation> GetUserLocation(User user);
        Task<List<UserReward>> GetUserRewards(User user);
        Task<VisitedLocation> TrackUserLocation(User user);
    }
}