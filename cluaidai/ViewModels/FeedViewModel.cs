using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cluaidai.Models;
using cluaidai.Services;

namespace cluaidai.ViewModels;

public partial class FeedViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private int _currentPage = 0;
    private const int PageSize = 10;

    [ObservableProperty]
    private ObservableCollection<Post> posts = new();

    [ObservableProperty]
    private bool isRefreshing;

    public FeedViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Feed";
    }

    public async Task InitializeAsync()
    {
        await LoadPostsAsync();
    }

    [RelayCommand]
    private async Task LoadPostsAsync()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            var newPosts = await _apiService.GetFeedAsync(0, PageSize);
            if (newPosts != null)
            {
                Posts.Clear();
                foreach (var post in newPosts)
                {
                    Posts.Add(post);
                }
                _currentPage = 0;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load posts: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadMorePostsAsync()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            _currentPage++;
            var newPosts = await _apiService.GetFeedAsync(_currentPage, PageSize);
            if (newPosts != null)
            {
                foreach (var post in newPosts)
                {
                    Posts.Add(post);
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load more posts: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleLikeAsync(Post post)
    {
        try
        {
            if (post.IsLiked)
            {
                var success = await _apiService.UnlikePostAsync(post.Id);
                if (success)
                {
                    post.IsLiked = false;
                    post.LikesCount--;
                }
            }
            else
            {
                var success = await _apiService.LikePostAsync(post.Id);
                if (success)
                {
                    post.IsLiked = true;
                    post.LikesCount++;
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to like post: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadPostsAsync();
    }

    [RelayCommand]
    private async Task CreatePostAsync()
    {
        await Shell.Current.GoToAsync("createpost");
    }

    [RelayCommand]
    private async Task NavigateToSearchAsync()
    {
        await Shell.Current.GoToAsync("search");
    }
}
