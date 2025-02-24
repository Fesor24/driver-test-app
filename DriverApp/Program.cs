// See https://aka.ms/new-console-template for more information
using DriverApp;

Console.WriteLine("Driver App!!!");

string baseUrl = "https://localhost:7175";

// paste access token here...
string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJodHRwczovL2Rldi5zb2xvcmlkZS5hcHAiLCJpc3MiOiJodHRwczovL2Rldi5zb2xvcmlkZS5hcHAiLCJleHAiOjUzNDAzODcwMzAsImlhdCI6MTc0MDM4NzAzMCwibmJmIjoxNzQwMzg3MDMwLCJlbWFpbCI6ImxleEBtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiRFJJVkVSSUQtMSIsImRyaXZlciI6IjEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJEcml2ZXIifQ.gsQMVaxXErwiE0m1yduZvNdqE_ey1dHmF2UiENpzxco";

using var rideHubService = new RideHubService(baseUrl, accessToken);

await rideHubService.StartAsync();

Task locationTask = UpdateLocation();

Task chatTask = Chat();

await Task.WhenAll(locationTask, chatTask);

async Task Chat()
{
    Console.WriteLine("Enter a message to chat if there's a ride match: ");
    string? message = await Task.Run(() => Console.ReadLine() ?? "");

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
            Longitude = 3.3898,
            Latitude = 6.5158
        });

        await Task.Delay(4000);
    }
}
