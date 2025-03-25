namespace DriverApp;

public sealed class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public sealed class CallNotification
{
    public string CreatedAt { get; set; }
    public int ExpiryInSeconds { get; set; }
    public long RideId { get; set; }
    public CallerInfo CallerInfo { get; set; }
    public CallInfo CallInfo { get; set; }

}

public class CallerInfo
{
    public string Caller { get; set; }
    public string Name { get; set; }
    public string ProfileImage { get; set; }
}

public class CallInfo
{
    public string Recipient { get; set; }
    public string Token { get; set; }
    public string Channel { get; set; }
}

// lat long
// address used=maryland mall, Maryland, Lagos...6.577117, 3.366819...used as pickup location
//


// nearby drivers
//6.575741, 3.369956
// 6.575613, 3.369870
// 6.575187, 3.369548
// 6.574931, 3.369291
// 6.575187, 3.369012
// 6.575421, 3.368540
// 6.575720, 3.368089
// 6.576658, 3.368046


// pickup to destination

