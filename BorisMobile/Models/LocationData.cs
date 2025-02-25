namespace BorisMobile.Models
{
    public class LocationData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public double Altitude { get; set; }
        public double VerticalAccuracy { get; set; }
        public double Speed { get; set; }
        public double Course { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Provider { get; set; }
    }
}
