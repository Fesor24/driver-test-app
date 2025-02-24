﻿using System.Text.Json;
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
    }

    public async Task StartAsync() => await _rideHub.StartAsync();

    public async Task UpdateLocation(Location location)
    {
        await _rideHub.InvokeAsync("UpdateLocation", location, CancellationToken.None);
        Console.WriteLine("Driver Location updated");
    }

    public async Task Chat(string message)
    {
        if (_rideId == default) return;

        SendChatMessage rideChat = new()
        {
            RideId = _rideId,
            Message = message
        };

        await _rideHub.InvokeAsync("Chat", rideChat, CancellationToken.None);
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

            Console.WriteLine($"Ride id is {request.RideId}");

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
                Console.WriteLine($"{chat.Sender} to {chat.Recipient}: {chat.Message}");
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
        });
    }

    private void Close()
    {
        _rideHub.StopAsync();
    }

    public void Dispose()
    {
        Close();
    }
}
