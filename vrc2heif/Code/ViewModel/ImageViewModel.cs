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
        conversionProgress = 0;
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

    private void imageConversion(string[] imagePaths)
    {
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
            ConversionProgress = ((double)_i / imagePaths.Length);
        }
    }

    [RelayCommand]
    void ScanForFiles()
    {
        // reset values
        int totalFileCount = 0;
        showQuickConvertButton = false;
        List<ImageModel> files = new();
        StatusMessage = "Loading...";

        int fileCount = Directory.GetFiles(Settings.SourcePath).Length;
        int folderCount = Directory.GetDirectories(Settings.SourcePath).Length;

        Debug.WriteLine($"Number of files: {fileCount}\nNumber of folders: {folderCount}");

        // TODO cache values
        for (int i = 0; i < Directory.GetDirectories(Settings.SourcePath).Length; i++)
        {
            totalFileCount += Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i])).Length;

            for (int j = 0; j < Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i])).Length; j++)
            {
                files.Add(new ImageModel() { PathName = Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i]))[j] });
                Debug.WriteLine(Directory.GetFiles(Path.Combine(Settings.SourcePath, Directory.GetDirectories(Settings.SourcePath)[i]))[j]);
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
