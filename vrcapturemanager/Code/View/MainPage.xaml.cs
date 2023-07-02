using vrcapturemanager.ViewModel;
namespace vrcapturemanager.View;

public partial class MainPage : ContentPage
{
    public MainPage(ImageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
