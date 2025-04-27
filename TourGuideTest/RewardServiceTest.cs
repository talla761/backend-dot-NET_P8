using GpsUtil.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourGuide.Users;
using TourGuide.Utilities;

namespace TourGuideTest;

public class RewardServiceTest : IClassFixture<DependencyFixture>
{
    private readonly DependencyFixture _fixture;

    public RewardServiceTest(DependencyFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UserGetRewards()
    {
        _fixture.Initialize(0);
        var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
        var attraction = (await _fixture.GpsUtil.GetAttractions()).First();
        user.AddToVisitedLocations(new VisitedLocation(user.UserId, attraction, DateTime.Now));
        await _fixture.TourGuideService.TrackUserLocation(user);
        var userRewards = user.UserRewards;
        _fixture.TourGuideService.Tracker.StopTracking();
        Assert.True(userRewards.Count == 1);
    }

    [Fact]
    public async Task IsWithinAttractionProximity()
    {
        var attraction = (await _fixture.GpsUtil.GetAttractions()).First();
        Assert.True(await _fixture.RewardsService.IsWithinAttractionProximity(attraction, attraction));
    }

    //[Fact(Skip = ("Needs fixed - can throw InvalidOperationException"))]
    //public async Task NearAllAttractions()
    //{
    //    _fixture.Initialize(1);
    //    _fixture.RewardsService.SetProximityBuffer(int.MaxValue);

    //    var user = _fixture.TourGuideService.GetAllUsers().First();
    //    _fixture.RewardsService.CalculateRewards(user);
    //    var userRewards = _fixture.TourGuideService.GetUserRewards(user);
    //    _fixture.TourGuideService.Tracker.StopTracking();

    //    Assert.Equal(_fixture.GpsUtil.GetAttractions().Count, userRewards.Count);
    //}

    [Fact()]
    public async Task NearAllAttractions()
    {
        _fixture.Initialize(1);
        _fixture.RewardsService.SetProximityBuffer(int.MaxValue);

        var user = (await _fixture.TourGuideService.GetAllUsers()).First();

        // Forcer un VisitedLocation à une attraction (optionnel mais plus sûr)
        var attractions = await _fixture.GpsUtil.GetAttractions();
        var location = new VisitedLocation(user.UserId, attractions[0], DateTime.UtcNow);
        user.AddToVisitedLocations(location);

        await _fixture.RewardsService.CalculateRewards(user);

        var useRewards = await _fixture.TourGuideService.GetUserRewards(user);

        _fixture.TourGuideService.Tracker.StopTracking();

        Assert.Equal(attractions.Count, useRewards.Count);
    }

}
