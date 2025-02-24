namespace DriverApp;

public sealed class RideUpdates
{
    public string Update { get; set; }
    public string Data { get; set; }
}

public sealed class DefaultUpdate
{
    public string Message { get; set;}
}

public static class ReceiveRideUpdate
{
    public static string Accepted = nameof(Accepted);
    public static string Cancelled = nameof(Cancelled);
    public static string NoMatch = nameof(NoMatch);
    public static string DriverArrived = nameof(DriverArrived);
    public static string Started = nameof(Started);
    public static string Ended = nameof(Ended);
    public static string Reassigned = nameof(Reassigned);
    public static string Rerouted = nameof(Rerouted);
}
