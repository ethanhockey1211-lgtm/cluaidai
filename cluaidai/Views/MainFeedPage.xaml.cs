using cluaidai.ViewModels;

namespace cluaidai.Views;

public partial class MainFeedPage : ContentPage
{
    private readonly FeedViewModel _viewModel;

    public MainFeedPage(FeedViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
