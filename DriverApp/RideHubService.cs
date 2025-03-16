using System.Text.Json;
using AsyncAwaitBestPractices;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace DriverApp;

public sealed class RideHubService : IDisposable
{
    private HubConnection _rideHub;

    private readonly string _connectionUrl;
    private readonly string _accessToken;
    private long _rideId;

    public RideHubService(string baseUrl, string accessToken)
    {
        _connectionUrl = baseUrl + "/ride-hub";
        _accessToken = accessToken;

        _rideHub = new HubConnectionBuilder()
            .WithUrl(_connectionUrl, options =>
            {
                options.Headers.Add("Authorization", "Bearer " + _accessToken);
            })
            .AddMessagePackProtocol()
            .WithKeepAliveInterval(TimeSpan.FromSeconds(20))
            .Build();

        SubscribeToChatRequestUpdates();
        SubscribeToRideUpdates();
        SubscribeToRideRequest();

        //StartAsync().SafeFireAndForget<Exception>(ex => Console.WriteLine($"An error occurred. {ex.Message}"));
    }

    public async Task StartAsync() => await _rideHub.StartAsync();

    public Task UpdateLocation(Location location)
    {
        Console.WriteLine("<--Driver Location updated-->");
        return _rideHub.InvokeAsync("UpdateLocation", location, CancellationToken.None);
    }

    public Task Chat(string message)
    {
        if (_rideId == default) return Task.CompletedTask;

        SendChatMessage rideChat = new()
        {
            RideId = _rideId,
            Message = message
        };

        return _rideHub.InvokeAsync("Chat", rideChat, CancellationToken.None);
    }

    private void SubscribeToRideRequest()
    {
        _rideHub.On<RideRequest>("ReceiveRideRequests", (request) =>
        {
            _rideId = request.RideId;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ride request received: Details:");
            Console.WriteLine($"RideId: {request.RideId}");
            Console.WriteLine($"Rider: {request.Rider.FirstName} {request.Rider.LastName}");
            Console.WriteLine($"Source: {request.Source}");
            Console.WriteLine($"Destination: {request.Destination}");
            Console.WriteLine($"Fare is {request.EstimatedFare}");
            Console.WriteLine("Waypoints addresses: ");

            _rideId = request.RideId;

            int count = 1;
            foreach (var item in request.Waypoints)
            {
                Console.WriteLine($"Address {count}. {item}");
            }

            Console.ForegroundColor = ConsoleColor.White;
        });
    }

    private void SubscribeToChatRequestUpdates()
    {
        _rideHub.On<RideChat>("ReceiveChatMessage", (request) =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Chats received");
            foreach (var chat in request.Chats)
            {
                Console.WriteLine($"{chat.Sender}: {chat.Message}");
            }

            Console.ForegroundColor = ConsoleColor.White;
        });
    }

    private void SubscribeToRideUpdates()
    {
        _rideHub.On<RideUpdates>("ReceiveRideUpdates", (request) =>
        {
            // listen for cancelled...
            if (request.Update == ReceiveRideUpdate.Cancelled)
            {
                var data = JsonSerializer.Deserialize<DefaultUpdate>(request.Data) ?? new();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(data.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (request.Update == ReceiveRideUpdate.RequestWaitTimeExtension)
            {
                var data = JsonSerializer.Deserialize<RequestWaitTimeExtension>(request.Data) ?? new();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(data.Message);
                Console.WriteLine($"Extension amount: {data.Amount}");
                Console.WriteLine($"Wait time Id is: {data.WaitTimeId}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        });
    }

    private void Close()
    {
        _rideHub.StopAsync().SafeFireAndForget();
    }

    public void Dispose()
    {
        Close();
    }
}
