﻿using GpsUtil.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourGuide.Services;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuideTest
{
    public class TourGuideServiceTour : IClassFixture<DependencyFixture>
    {
        private readonly DependencyFixture _fixture;

        public TourGuideServiceTour(DependencyFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
            _fixture.Cleanup();
        }

        [Fact]
        public async Task GetUserLocation()
        {
            _fixture.Initialize(0);
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var visitedLocation = await _fixture.TourGuideService.TrackUserLocation(user);
            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(user.UserId, visitedLocation.UserId);
        }

        [Fact]
        public async Task AddUser()
        {
            _fixture.Initialize(0);
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var user2 = new User(Guid.NewGuid(), "jon2", "000", "jon2@tourGuide.com");

            _fixture.TourGuideService.AddUser(user);
            _fixture.TourGuideService.AddUser(user2);

            var retrievedUser = await _fixture.TourGuideService.GetUser(user.UserName);
            var retrievedUser2 = await _fixture.TourGuideService.GetUser(user2.UserName);

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(user, retrievedUser);
            Assert.Equal(user2, retrievedUser2);

        }

        [Fact]
        public async Task GetAllUsers()
        {
            _fixture.Initialize(0);
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var user2 = new User(Guid.NewGuid(), "jon2", "000", "jon2@tourGuide.com");

            _fixture.TourGuideService.AddUser(user);
            _fixture.TourGuideService.AddUser(user2);

            List<User> allUsers = await _fixture.TourGuideService.GetAllUsers();

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Contains(user, allUsers);
            Assert.Contains(user2, allUsers);
        }

        [Fact]
        public async Task TrackUser()
        {
            _fixture.Initialize();
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var visitedLocation = await _fixture.TourGuideService.TrackUserLocation(user);

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(user.UserId, visitedLocation.UserId);
        }

        //[Fact(Skip = "Not yet implemented")]
        [Fact]
        public async Task GetNearbyAttractions()
        {
            _fixture.Initialize(0);
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var visitedLocation = await _fixture.TourGuideService.TrackUserLocation(user);

            var nearbyAttractions = await _fixture.TourGuideService.GetNearByAttractions(visitedLocation, user);

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(5, nearbyAttractions.Count);
        }

        [Fact]
        public async Task GetTripDeals()
        {
            _fixture.Initialize(0);
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            List<Provider> providers = await _fixture.TourGuideService.GetTripDeals(user);

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(10, providers.Count);
        }
    }
}
