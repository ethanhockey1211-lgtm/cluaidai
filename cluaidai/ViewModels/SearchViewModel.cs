using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cluaidai.Models;
using cluaidai.Services;

namespace cluaidai.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<User> searchResults = new();

    [ObservableProperty]
    private bool isSearching;

    public SearchViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Search";
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SearchResults.Clear();
        }
        else
        {
            _ = PerformSearchAsync();
        }
    }

    [RelayCommand]
    private async Task PerformSearchAsync()
    {
        if (IsSearching || string.IsNullOrWhiteSpace(SearchQuery))
            return;

        IsSearching = true;

        try
        {
            var results = await _apiService.SearchUsersAsync(SearchQuery);
            if (results != null)
            {
                SearchResults.Clear();
                foreach (var user in results)
                {
                    SearchResults.Add(user);
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to search users: {ex.Message}", "OK");
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SelectUserAsync(User user)
    {
        if (user == null) return;

        // Navigate to user's profile or start a conversation
        await Shell.Current.GoToAsync($"conversation?userId={user.Id}");
    }

    [RelayCommand]
    private async Task ToggleFollowAsync(User user)
    {
        if (user == null) return;

        try
        {
            if (user.IsFollowing)
            {
                var success = await _apiService.UnfollowUserAsync(user.Id);
                if (success)
                {
                    user.IsFollowing = false;
                    user.FollowersCount--;
                }
            }
            else
            {
                var success = await _apiService.FollowUserAsync(user.Id);
                if (success)
                {
                    user.IsFollowing = true;
                    user.FollowersCount++;
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to update follow status: {ex.Message}", "OK");
        }
    }
}
