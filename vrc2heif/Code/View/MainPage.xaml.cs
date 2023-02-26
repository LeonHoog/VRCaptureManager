using SixLabors.ImageSharp;
using LibHeifSharp;
using System.Diagnostics;
using Image = Microsoft.Maui.Controls.Image;
using vrc2heif.ViewModel;

namespace vrc2heif.View;

public partial class MainPage : ContentPage
{
    int count;

    public MainPage(ImageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
