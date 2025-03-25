using System.Text.Json;
using DriverApp;

Console.WriteLine("Driver App!!!");

string devUrl = "https://localhost:7175";
string stagingUrl = "https://stagingapi.soloride.app";

string filePath = "token.txt";

string fileContent;

string accessToken;

double[,] tripLocationUpdate = new double[8, 2]
{
    { 6.575741, 3.369956 },
    { 6.575613, 3.369870 },
    { 6.575187, 3.369548 },
    { 6.574931, 3.369291 },
    { 6.575187, 3.369012 },
    { 6.575421, 3.368540 },
    { 6.575720, 3.368089 },
    { 6.576658, 3.368046 }
};

try
{
    using var streamReader = new StreamReader(filePath);

    fileContent = streamReader.ReadToEnd();
}
catch(FileNotFoundException)
{
    fileContent = string.Empty;
}

Console.WriteLine("Enter 'y' to use cached data or 'n' to paste new token? [y/n]");

string response = Console.ReadLine();

if(response == "y")
{
    accessToken = await GetOrUpdateTokenIfExpired(fileContent, filePath);
}
else
{
    accessToken = GetToken();
    await SaveToken(accessToken, filePath);
}

Console.Clear();

Console.WriteLine("Which env are you working with?");
Console.WriteLine("1. dev");
Console.WriteLine("2. staging");
Console.WriteLine("Default 'env' is staging");
string env = Console.ReadLine() ?? "";

string url = stagingUrl;

if (env.Equals("dev", StringComparison.OrdinalIgnoreCase))
    url = devUrl;

Console.Clear();

Console.WriteLine("To update your current location. Input location information below:");
Console.WriteLine("Your Latitude: ");
if (!double.TryParse(Console.ReadLine(), out double lat)) lat = 0;

Console.WriteLine("Your Longitude: ");
if (!double.TryParse(Console.ReadLine(), out double longitude)) longitude = 0;

Console.Clear();

using var rideHubService = new RideHubService(url, accessToken);

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
        //if(rideHubService.RideId != default)
        //{
        //    int locationCount = tripLocationUpdate.GetLength(0);

        //    for(int i = 0; i < locationCount; i++)
        //    {
        //        await rideHubService.UpdateLocation(new Location
        //        {
        //            Longitude = tripLocationUpdate[i, 1],
        //            Latitude = tripLocationUpdate[i, 0]
        //        });

        //        await Task.Delay(5000);

        //        if (i == locationCount - 1)
        //            i = 0;
        //    }

        //}
        //else
        //{
        //    await rideHubService.UpdateLocation(new Location
        //    {
        //        Longitude = longitude,
        //        Latitude = lat
        //    });
        //}


        await rideHubService.UpdateLocation(new Location
        {
            Longitude = longitude,
            Latitude = lat
        });

        await Task.Delay(4000);
    }
}

string GetToken()
{
    Console.WriteLine("Paste drivers access token below: ");
    string? accessToken = Console.ReadLine();

    while (string.IsNullOrWhiteSpace(accessToken))
        accessToken = Console.ReadLine();

    return accessToken;
}

async Task<string> GetOrUpdateTokenIfExpired(string content, string path)
{
    string? accessToken;

    if (string.IsNullOrWhiteSpace(content))
    {
        Console.WriteLine("No saved data");
        accessToken = GetToken();

        content = JsonSerializer.Serialize(new CachedData(accessToken, DateTime.UtcNow.AddHours(14)));

        using var streamWriter = new StreamWriter(path);

        await streamWriter.WriteLineAsync(content);
    }
    else
    {
        var cachedData = JsonSerializer.Deserialize<CachedData>(content);

        if (cachedData is null || cachedData.Expiry <= DateTime.UtcNow)
        {
            Console.WriteLine("Token expired");
            accessToken = GetToken();

            content = JsonSerializer.Serialize(new CachedData(accessToken, DateTime.UtcNow.AddHours(14)));

            using var streamWriter = new StreamWriter(path);

            await streamWriter.WriteLineAsync(content);
        }
        else
        {
            accessToken = cachedData.AccessToken;
        }
    }

    return accessToken;
}

async Task SaveToken(string accessToken, string path)
{
    string content = JsonSerializer.Serialize(new CachedData(accessToken, DateTime.UtcNow.AddHours(14)));

    using var streamWriter = new StreamWriter(path);

    await streamWriter.WriteLineAsync(content);
}

sealed record CachedData(string AccessToken, DateTime Expiry);