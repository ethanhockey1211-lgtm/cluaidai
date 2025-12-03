using cluaidai.Services;

namespace cluaidai.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ProfileViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Profile";
    }
}
