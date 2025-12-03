using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cluaidai.Services;

namespace cluaidai.ViewModels;

public partial class CreatePostViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string caption = string.Empty;

    [ObservableProperty]
    private string? imageUrl;

    [ObservableProperty]
    private bool isPosting;

    [ObservableProperty]
    private bool hasImage;

    public CreatePostViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Create Post";
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();

                // Upload the image
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                ImageUrl = await _apiService.UploadImageAsync(imageBytes, photo.FileName);
                HasImage = !string.IsNullOrEmpty(ImageUrl);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();

                // Upload the image
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                ImageUrl = await _apiService.UploadImageAsync(imageBytes, photo.FileName);
                HasImage = !string.IsNullOrEmpty(ImageUrl);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to take photo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void RemoveImage()
    {
        ImageUrl = null;
        HasImage = false;
    }

    [RelayCommand]
    private async Task CreatePostAsync()
    {
        if (IsPosting || (string.IsNullOrWhiteSpace(Caption) && string.IsNullOrEmpty(ImageUrl)))
        {
            await Shell.Current.DisplayAlert("Error", "Please add a caption or image", "OK");
            return;
        }

        IsPosting = true;

        try
        {
            var post = await _apiService.CreatePostAsync(Caption, ImageUrl);
            if (post != null)
            {
                await Shell.Current.DisplayAlert("Success", "Post created successfully!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to create post", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to create post: {ex.Message}", "OK");
        }
        finally
        {
            IsPosting = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
