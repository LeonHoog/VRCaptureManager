using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMagick;
using System.Diagnostics;
using vrc2heif.Model;

namespace vrc2heif.ViewModel;

public partial class ImageViewModel : ObservableObject
{
    private readonly Settings settings;
    public ImageViewModel(Settings settings)
    {
        this.settings = settings;
        showQuickConvertButton = false;
        statusMessage = "Search for pictures";
        imageWidth = 400;
        imagesInRow = (int)(DeviceDisplay.MainDisplayInfo.Width / imageWidth);
    }

    [ObservableProperty]
    bool showQuickConvertButton;

    [ObservableProperty]
    string statusMessage;

    [ObservableProperty]
    ImageModel[] images;

    [ObservableProperty]
    int imageWidth;

    [ObservableProperty]
    int imagesInRow;

    [RelayCommand]
    static void QuickConvert(string imagePath)
    {
        // Load the PNG image
        using MagickImage pngImage = new(imagePath);
        pngImage.Format = MagickFormat.WebP;

        // Save the HEIF image to a file
        pngImage.Write(imagePath);
    }

    [RelayCommand]
    void ScanForFiles()
    {
        ShowQuickConvertButton = false;
        StatusMessage = "Loading...";

        int totalFileCount = 0;

        int fileCount = Directory.GetFiles(Settings.SourcePath).Length;
        int folderCount = Directory.GetDirectories(Settings.SourcePath).Length;

        //flexLayout.Children.Clear();

        Debug.WriteLine($"Number of files: {fileCount}");
        Debug.WriteLine($"Number of folders: {folderCount}");

        List<ImageModel> files = new();

        // TODO cache values
        for (int i = 0; i < Directory.GetDirectories(Settings.SourcePath).Length; i++)
        {
            totalFileCount += Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i])).Length;

            for (int j = 0; j < Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i])).Length; j++)
            {
                files.Add(new ImageModel() { PathName = Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i]))[j] });
            }

        }

        Images = files.ToArray();

        StatusMessage = $"{totalFileCount} files detected";

        ShowQuickConvertButton = true;
    }

    [RelayCommand]
    async static void OnActionSheetFileLocation()
    {
        Settings.RetrieveData();
        string result = await Application.Current.MainPage.DisplayPromptAsync("File Location", "VRC Pictures (Documents folder)", initialValue: Settings.SourcePath, keyboard: Keyboard.Text).ConfigureAwait(false);

        if (result == null)
            Debug.WriteLine("Cancel");
        else if (string.IsNullOrWhiteSpace(result))
            Debug.WriteLine("OK, without input");
        else
        {
            Settings.SourcePath = result;
            Settings.SaveData();
        }
    }
}
