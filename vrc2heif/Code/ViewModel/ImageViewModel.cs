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
        enableFilesScanButton = true;
        enableQuickConvertButton = false;
        statusMessageScan = "Search for pictures";
        statusMessageConvert = "Quick Convert";
        imageWidth = 400;
        conversionProgress = 0;
    }

    [ObservableProperty]
    bool enableFilesScanButton;

    [ObservableProperty]
    bool enableQuickConvertButton;

    [ObservableProperty]
    string statusMessageScan;

    [ObservableProperty]
    string statusMessageConvert;

    [ObservableProperty]
    ImageModel[] images;

    [ObservableProperty]
    int imageWidth;

    [ObservableProperty]
    double conversionProgress;

    [ObservableProperty]
    static List<ImageModel> filesList;

    [RelayCommand]
    void QuickConvert(ImageModel[] images)
    {
        string[] imagePaths = new string[images.Length];
        for (int i = 0; i < images.Length; i++) { imagePaths[i] = images[i].PathName; }
        imageConversion(imagePaths);
    }

    private async void imageConversion(string[] imagePaths)
    {
        await Task.Run(() =>
        {
            EnableQuickConvertButton = false;
            EnableFilesScanButton = false;
            ConversionProgress = 0;
            for (int _i = 0; _i < imagePaths.Length; _i++)
            {
                string outputDirectory = Path.GetDirectoryName(imagePaths[_i]);
                string outputFileName = Path.GetFileNameWithoutExtension(imagePaths[_i]) + ".webp";
                string outputImagePath = Path.Combine(outputDirectory, outputFileName);

                using (MagickImage image = new MagickImage(imagePaths[_i]))
                {
                    image.Format = MagickFormat.WebP;
                    image.Quality = 100;
                    image.Write(outputImagePath);
                }
                ConversionProgress = ((double)(_i+1) / imagePaths.Length);
            }
        });

        // Update the UI on the main (UI) thread
        await Device.InvokeOnMainThreadAsync(() =>
        {
            StatusMessageConvert = "Done!";
            EnableFilesScanButton = true;
        });
    }

    [RelayCommand]
    async void ScanForFiles()
    {
        // reset values
        int totalFileCount = 0;
        enableQuickConvertButton = false;
        List<ImageModel> files = new();
        StatusMessageScan = "Loading...";

        await Task.Run(() =>
        {
            int fileCount = Directory.GetFiles(Settings.SourcePath).Length;
            int folderCount = Directory.GetDirectories(Settings.SourcePath).Length;

            Debug.WriteLine($"Number of files: {fileCount}\nNumber of folders: {folderCount}");

            // TODO cache values
            for (int i = 0; i < Directory.GetDirectories(Settings.SourcePath).Length; i++)
            {
                for (int j = 0; j < Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i])).Length; j++)
                {
                    bool isMatch = false;

                    switch (Path.GetExtension(Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i]))[j])) {
                        case ".png":
                            isMatch = true;
                            break;
                    }
                    if (isMatch) {
                        files.Add(new ImageModel() { PathName = Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i]))[j] });
                        totalFileCount++;
                    }
                }
            }
        });

        // Update the UI on the main (UI) thread
        await Device.InvokeOnMainThreadAsync(() =>
        {
            Images = files.ToArray();
            StatusMessageScan = $"{totalFileCount} files detected";
            enableQuickConvertButton = true;
        });
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
