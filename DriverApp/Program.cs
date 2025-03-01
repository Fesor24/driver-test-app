using System.Text.Json;
using DriverApp;

Console.WriteLine("Driver App!!!");

string baseUrl = "https://stagingapi.soloride.app";

string filePath = "token.txt";

string fileContent;

string accessToken;

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

        content = JsonSerializer.Serialize(new CachedData(accessToken, DateTime.UtcNow.AddHours(4)));

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

            content = JsonSerializer.Serialize(new CachedData(accessToken, DateTime.UtcNow.AddHours(4)));

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
    string content = JsonSerializer.Serialize(new CachedData(accessToken, DateTime.UtcNow.AddHours(4)));

    using var streamWriter = new StreamWriter(path);

    await streamWriter.WriteLineAsync(content);
}

sealed record CachedData(string AccessToken, DateTime Expiry);