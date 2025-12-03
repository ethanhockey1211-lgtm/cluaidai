using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cluaidai.Services;

namespace cluaidai.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly SignalRService _signalRService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private bool isRegisterMode;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginViewModel(ApiService apiService, AuthService authService, SignalRService signalRService)
    {
        _apiService = apiService;
        _authService = authService;
        _signalRService = signalRService;
        Title = "Welcome to Norta";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiService.LoginAsync(Email, Password);
            if (response != null)
            {
                await _authService.SetAuthenticationAsync(response);
                await _signalRService.ConnectAsync();
                await Shell.Current.GoToAsync("//MainFeed");
            }
            else
            {
                ErrorMessage = "Invalid email or password";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiService.RegisterAsync(Email, Password, DisplayName);
            if (response != null)
            {
                await _authService.SetAuthenticationAsync(response);
                await _signalRService.ConnectAsync();
                await Shell.Current.GoToAsync("//MainFeed");
            }
            else
            {
                ErrorMessage = "Failed to register. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
        ErrorMessage = string.Empty;
    }
}
