using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpsUtil.Location;

public class Locations
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }

    public Locations(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}
