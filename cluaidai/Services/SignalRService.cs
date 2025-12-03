using Microsoft.AspNetCore.SignalR.Client;
using cluaidai.Models;

namespace cluaidai.Services;

public class SignalRService
{
    private HubConnection? _chatConnection;
    private HubConnection? _notificationsConnection;
    private readonly AuthService _authService;
    private readonly string _baseUrl = "https://localhost:5001"; // Configure this

    public event EventHandler<Message>? MessageReceived;
    public event EventHandler<Message>? MessageSent;
    public event EventHandler<Notification>? NotificationReceived;
    public event Action<Message>? OnMessageReceived;

    public SignalRService(AuthService authService)
    {
        _authService = authService;
    }

    public async Task StartAsync()
    {
        await ConnectAsync();
    }

    public async Task ConnectAsync()
    {
        var token = await _authService.GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token)) return;

        // Chat Hub
        _chatConnection = new HubConnectionBuilder()
            .WithUrl($"{_baseUrl}/hubs/chat?access_token={token}")
            .WithAutomaticReconnect()
            .Build();

        _chatConnection.On<Message>("ReceiveMessage", (message) =>
        {
            MessageReceived?.Invoke(this, message);
            OnMessageReceived?.Invoke(message);
        });

        _chatConnection.On<Message>("MessageSent", (message) =>
        {
            MessageSent?.Invoke(this, message);
        });

        await _chatConnection.StartAsync();

        // Notifications Hub
        _notificationsConnection = new HubConnectionBuilder()
            .WithUrl($"{_baseUrl}/hubs/notifications?access_token={token}")
            .WithAutomaticReconnect()
            .Build();

        _notificationsConnection.On<Notification>("ReceiveNotification", (notification) =>
        {
            NotificationReceived?.Invoke(this, notification);
        });

        await _notificationsConnection.StartAsync();
    }

    public async Task SendMessageAsync(string toUserId, string text)
    {
        if (_chatConnection?.State == HubConnectionState.Connected)
        {
            await _chatConnection.InvokeAsync("SendMessage", toUserId, text);
        }
    }

    public async Task MarkAsReadAsync(Guid messageId)
    {
        if (_chatConnection?.State == HubConnectionState.Connected)
        {
            await _chatConnection.InvokeAsync("MarkAsRead", messageId);
        }
    }

    public async Task DisconnectAsync()
    {
        if (_chatConnection != null)
        {
            await _chatConnection.StopAsync();
            await _chatConnection.DisposeAsync();
        }

        if (_notificationsConnection != null)
        {
            await _notificationsConnection.StopAsync();
            await _notificationsConnection.DisposeAsync();
        }
    }
}
