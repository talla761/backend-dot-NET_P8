﻿using TourGuide.LibrairiesWrappers.Interfaces;

namespace TourGuide.LibrairiesWrappers
{
    public class RewardCentralWrapper : IRewardCentral
    {
        private readonly RewardCentral.RewardCentral _rewardCentral;

        public RewardCentralWrapper()
        {
            _rewardCentral = new ();
        }

        public async Task<int> GetAttractionRewardPoints(Guid attractionId, Guid userId)
        {
            return await _rewardCentral.GetAttractionRewardPoints(attractionId, userId);
        }
    }
}
