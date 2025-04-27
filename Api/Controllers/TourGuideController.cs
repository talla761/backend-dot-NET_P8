using GpsUtil.Location;
using Microsoft.AspNetCore.Mvc;
using TourGuide.Dto;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TripPricer;

namespace TourGuide.Controllers;

[ApiController]
[Route("[controller]")]
public class TourGuideController : ControllerBase
{
    private readonly ITourGuideService _tourGuideService;

    public TourGuideController(ITourGuideService tourGuideService)
    {
        _tourGuideService = tourGuideService;
    }

    [HttpGet("getLocation")]
    public async Task<ActionResult<VisitedLocation>> GetLocation([FromQuery] string userName)
    {
        var location = await _tourGuideService.GetUserLocation(await _tourGuideService.GetUser(userName));
        return Ok(location);
    }

    // TODO: Change this method to no longer return a List of Attractions.
    // Instead: Get the closest five tourist attractions to the user - no matter how far away they are.
    // Return a new JSON object that contains:
    // Name of Tourist attraction, 
    // Tourist attractions lat/long, 
    // The user's location lat/long, 
    // The distance in miles between the user's location and each of the attractions.
    // The reward points for visiting each Attraction.
    //    Note: Attraction reward points can be gathered from RewardsCentral

    [HttpGet("getNearbyAttractions")]
    public async Task<ActionResult<List<NearbyAttractionDto>>> GetNearbyAttractions([FromQuery] string userName)
    {
        var user = await _tourGuideService.GetUser(userName);
        var visitedLocation = await _tourGuideService.GetUserLocation(user);
        var nearbyAttractions = await _tourGuideService.GetNearByAttractions(visitedLocation, user);

        return Ok(nearbyAttractions);
    }

    [HttpGet("getRewards")]
    public async Task<ActionResult<List<UserReward>>> GetRewards([FromQuery] string userName)
    {
        var rewards = await _tourGuideService.GetUserRewards(await _tourGuideService.GetUser(userName));
        return Ok(rewards);
    }

    [HttpGet("getTripDeals")]
    public async Task<ActionResult<List<Provider>>> GetTripDeals([FromQuery] string userName)
    {
        var deals = await _tourGuideService.GetTripDeals(await _tourGuideService.GetUser(userName));
        return Ok(deals);
    }

    private async Task<User> GetUser(string userName)
    {
        return await _tourGuideService.GetUser(userName);
    }
}
