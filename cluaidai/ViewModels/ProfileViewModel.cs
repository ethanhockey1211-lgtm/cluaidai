using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cluaidai.Models;
using cluaidai.Services;

namespace cluaidai.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private User? currentUser;

    [ObservableProperty]
    private ObservableCollection<Post> userPosts = new();

    [ObservableProperty]
    private bool isLoadingProfile;

    [ObservableProperty]
    private bool isLoadingPosts;

    public ProfileViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Profile";
    }

    public async Task InitializeAsync()
    {
        await LoadProfileAsync();
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        if (IsLoadingProfile) return;

        IsLoadingProfile = true;

        try
        {
            CurrentUser = await _apiService.GetCurrentUserAsync();
            await LoadUserPostsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
        }
        finally
        {
            IsLoadingProfile = false;
        }
    }

    [RelayCommand]
    private async Task LoadUserPostsAsync()
    {
        if (IsLoadingPosts || CurrentUser == null) return;

        IsLoadingPosts = true;

        try
        {
            // This would normally call an endpoint to get user's posts
            // For now, we'll just clear the collection
            UserPosts.Clear();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load posts: {ex.Message}", "OK");
        }
        finally
        {
            IsLoadingPosts = false;
        }
    }

    [RelayCommand]
    private async Task EditProfileAsync()
    {
        await Shell.Current.DisplayAlert("Edit Profile", "This feature is coming soon!", "OK");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var result = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
        if (result)
        {
            await _apiService.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
