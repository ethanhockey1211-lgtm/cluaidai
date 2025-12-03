using cluaidai.Services;

namespace cluaidai.ViewModels;

public partial class ChatViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly SignalRService _signalRService;

    public ChatViewModel(ApiService apiService, SignalRService signalRService)
    {
        _apiService = apiService;
        _signalRService = signalRService;
        Title = "Messages";
    }
}
