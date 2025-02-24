namespace DriverApp;

public sealed class RideRequest
{
    public string Source { get; set; }
    public string Destination { get; set; }
    public List<string> Waypoints { get; set; }
    public long RideId { get; set; }
    public long EstimatedFare { get; set; }
    public RiderInfo Rider { get; set; }
}

public sealed class RiderInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNo { get; set; }
    public string ProfileImageUrl { get; set; }
}
