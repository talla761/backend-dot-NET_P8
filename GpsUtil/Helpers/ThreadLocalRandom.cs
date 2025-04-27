using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpsUtil.Helpers;

internal static class ThreadLocalRandom
{
    private static readonly ThreadLocal<Random> threadLocal = new ThreadLocal<Random>(() => new Random());

    public static Random Current => threadLocal.Value;

    public static async Task<double> NextDouble(double minValue, double maxValue)
    {
        return await Task.FromResult(Current.NextDouble() * (maxValue - minValue) + minValue);
    }

    //public static int Next(int minValue, int maxValue)
    //{
    //    return Current.Next(minValue, maxValue);
    //}
}

