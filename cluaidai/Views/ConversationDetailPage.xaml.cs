using cluaidai.ViewModels;

namespace cluaidai.Views;

public partial class ConversationDetailPage : ContentPage
{
    public ConversationDetailPage(ConversationDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ConversationDetailViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
