using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cluaidai.Models;
using cluaidai.Services;

namespace cluaidai.ViewModels;

public partial class ChatViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly SignalRService _signalRService;

    [ObservableProperty]
    private ObservableCollection<Conversation> conversations = new();

    [ObservableProperty]
    private bool isLoadingConversations;

    public ChatViewModel(ApiService apiService, SignalRService signalRService)
    {
        _apiService = apiService;
        _signalRService = signalRService;
        Title = "Messages";
    }

    public async Task InitializeAsync()
    {
        await LoadConversationsAsync();
    }

    [RelayCommand]
    private async Task LoadConversationsAsync()
    {
        if (IsLoadingConversations) return;

        IsLoadingConversations = true;

        try
        {
            var conversations = await _apiService.GetConversationsAsync();
            if (conversations != null)
            {
                Conversations.Clear();
                foreach (var conversation in conversations)
                {
                    Conversations.Add(conversation);
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load conversations: {ex.Message}", "OK");
        }
        finally
        {
            IsLoadingConversations = false;
        }
    }

    [RelayCommand]
    private async Task OpenConversationAsync(Conversation conversation)
    {
        if (conversation?.OtherUser == null) return;

        await Shell.Current.GoToAsync($"conversation?userId={conversation.OtherUser.Id}");
    }

    [RelayCommand]
    private async Task SearchUsersAsync()
    {
        await Shell.Current.GoToAsync("search");
    }
}
