using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cluaidai.Models;
using cluaidai.Services;

namespace cluaidai.ViewModels;

[QueryProperty(nameof(UserId), "userId")]
public partial class ConversationDetailViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly SignalRService _signalRService;

    [ObservableProperty]
    private string userId = string.Empty;

    [ObservableProperty]
    private User? otherUser;

    [ObservableProperty]
    private ObservableCollection<Message> messages = new();

    [ObservableProperty]
    private string messageText = string.Empty;

    [ObservableProperty]
    private bool isLoadingMessages;

    [ObservableProperty]
    private bool isSending;

    public ConversationDetailViewModel(ApiService apiService, SignalRService signalRService)
    {
        _apiService = apiService;
        _signalRService = signalRService;
        Title = "Chat";
    }

    public async Task InitializeAsync()
    {
        await LoadConversationAsync();
        await ConnectToSignalRAsync();
    }

    [RelayCommand]
    private async Task LoadConversationAsync()
    {
        if (IsLoadingMessages || string.IsNullOrEmpty(UserId)) return;

        IsLoadingMessages = true;

        try
        {
            var user = await _apiService.GetUserAsync(UserId);
            if (user != null)
            {
                OtherUser = user;
                Title = user.DisplayName;
            }

            var messages = await _apiService.GetConversationAsync(UserId);
            if (messages != null)
            {
                Messages.Clear();
                foreach (var message in messages)
                {
                    Messages.Add(message);
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load conversation: {ex.Message}", "OK");
        }
        finally
        {
            IsLoadingMessages = false;
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (IsSending || string.IsNullOrWhiteSpace(MessageText) || string.IsNullOrEmpty(UserId))
            return;

        IsSending = true;
        var text = MessageText;
        MessageText = string.Empty;

        try
        {
            await _signalRService.SendMessageAsync(UserId, text);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to send message: {ex.Message}", "OK");
            MessageText = text;
        }
        finally
        {
            IsSending = false;
        }
    }

    private async Task ConnectToSignalRAsync()
    {
        try
        {
            _signalRService.OnMessageReceived += OnMessageReceived;
            await _signalRService.StartAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to connect to chat: {ex.Message}", "OK");
        }
    }

    private void OnMessageReceived(Message message)
    {
        if (message.FromUserId == UserId || message.ToUserId == UserId)
        {
            Messages.Add(message);
        }
    }
}
