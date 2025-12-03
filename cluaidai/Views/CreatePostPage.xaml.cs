using cluaidai.ViewModels;

namespace cluaidai.Views;

public partial class CreatePostPage : ContentPage
{
    public CreatePostPage(CreatePostViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
