using RewardCentral.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardCentral;

public class RewardCentral
{
    private static readonly Random _random = new();

    public async Task<int> GetAttractionRewardPoints(Guid attractionId, Guid userId)
    {
        int randomDelay = _random.Next(1, 1000);
        await Task.Delay(randomDelay); // Simule une latence réseau

        int randomInt = _random.Next(1, 1000);
        return randomInt;
    }
}

