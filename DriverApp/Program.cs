
using DriverApp;

Console.WriteLine("Driver App!!!");

string baseUrl = "https://stagingapi.soloride.app";

Console.WriteLine("Paste drivers access token below: ");
string? accessToken = Console.ReadLine();

while(string.IsNullOrWhiteSpace(accessToken))
    accessToken = Console.ReadLine();

Console.Clear();

Console.WriteLine("To update your current location. Input location information below:");
Console.WriteLine("Your Latitude: ");
if (!double.TryParse(Console.ReadLine(), out double lat)) lat = 0;

Console.WriteLine("Your Longitude: ");
if (!double.TryParse(Console.ReadLine(), out double longitude)) longitude = 0;

using var rideHubService = new RideHubService(baseUrl, accessToken);

await rideHubService.StartAsync();

Task locationTask = UpdateLocation();

Task chatTask = Chat();

await Task.WhenAll(locationTask, chatTask).ConfigureAwait(ConfigureAwaitOptions.None);

async Task Chat()
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("Info: Enter a message to chat if there's a ride match: ");
    Console.ForegroundColor = ConsoleColor.White;
    string? message = await Console.In.ReadLineAsync() ?? "";

    while (!string.IsNullOrEmpty(message))
    {
        await rideHubService.Chat(message);

        Console.WriteLine("Enter a message: ");
        message = await Console.In.ReadLineAsync();
    }
}

async Task UpdateLocation()
{
    while (true)
    {
        await rideHubService.UpdateLocation(new Location
        {
            Longitude = longitude,
            Latitude = lat
        });

        await Task.Delay(4000);
    }
}
