using GpsUtil.Location;

namespace TourGuide.Dto
{
    public class NearbyAttractionDto
    {
        public string AttractionName { get; set; }
        public Locations AttractionLocation { get; set; }
        public Locations UserLocation { get; set; }
        public double DistanceInMiles { get; set; }
        public int RewardPoints { get; set; }
    }

}
