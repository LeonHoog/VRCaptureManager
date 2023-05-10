using vrc2heif.ViewModel;
namespace vrc2heif.View;

public partial class MainPage : ContentPage
{
    public MainPage(ImageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
