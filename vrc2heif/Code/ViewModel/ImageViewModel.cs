using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Diagnostics;
using vrc2heif.Model;
using Image = SixLabors.ImageSharp.Image;

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
        EnableQuickConvertButton = false;
        EnableFilesScanButton = false;
        ConversionProgress = 0;
        StatusMessageConvert = "Converting...";
        int count = 0;

        await Task.Run(() =>
        {
            Parallel.ForEach(imagePaths, (imagePath) =>
            {
                string outputImagePath = Path.ChangeExtension(imagePath, "webp");

                using Image image = Image.Load(imagePath);
                
                image.Save(outputImagePath, new WebpEncoder() { Quality = 100});
                ConversionProgress = (double)count++ / imagePaths.Length;
            });
        }).ConfigureAwait(false);

        ConversionProgress = 100;

        StatusMessageConvert = "Done!";
        EnableFilesScanButton = true;
    }

    [RelayCommand]
    void ScanForFiles()
    {
        // reset values
        EnableQuickConvertButton = false;
        List<ImageModel> files = new();
        StatusMessageScan = "Loading...";

        string[] filePaths = Directory.GetFiles(settings.SourcePath, "*.*", SearchOption.AllDirectories);

        foreach (string file in filePaths)
        {
            switch (Path.GetExtension(file))
            {
                case ".png":
                    files.Add(new ImageModel() { PathName = file });
                    break;
            }
        }

        // Update the UI on the main (UI) thread
        Debug.WriteLine($"Files counted: {files.Count}, path count: {filePaths.Length}");
        Images = files.ToArray();
        StatusMessageScan = $"{files.Count} files detected";
        EnableQuickConvertButton = true;
    }

    [RelayCommand]
    async void OnActionSheetFileLocation()
    {
        settings.RetrieveData();
        string result = await Application.Current.MainPage.DisplayPromptAsync("File Location", "VRC Pictures (Documents folder)", initialValue: settings.SourcePath, keyboard: Keyboard.Text);

        if (result == null)
            Debug.WriteLine("Cancel");
        else if (string.IsNullOrWhiteSpace(result))
            Debug.WriteLine("OK, without input");
        else
        {
            settings.SourcePath = result;
            settings.SaveData();
        }
    }
}
