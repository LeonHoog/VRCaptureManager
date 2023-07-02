using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SixLabors.ImageSharp.Formats.Webp;
using System.Diagnostics;
using vrcapturemanager.Model;
using vrcapturemanager.Resources.Localizations; 
using Image = SixLabors.ImageSharp.Image;

namespace vrcapturemanager.ViewModel;

public partial class ImageViewModel : ObservableObject
{
    private readonly Settings settings;
    public ImageViewModel(Settings settings)
    {
        this.settings = settings.RetrieveData();
        enableFilesScanButton = true;
        enableQuickConvertButton = false;
        statusMessageScan = LocalizationResource.search_items;
        statusMessageConvert = LocalizationResource.quick_convert;
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
        for (int i = 0; i < images.Length; i++)
            { imagePaths[i] = images[i].PathName; }

        switch (settings.OutputType)
        {
            case OutputTypes.webp:
                {
                    imageConversionWebp(imagePaths);
                    break;
                }
        }
    }

    private async void imageConversionWebp(string[] imagePaths)
    {
        EnableQuickConvertButton = false;
        EnableFilesScanButton = false;
        ConversionProgress = 0;
        StatusMessageConvert = "Converting..."; //TODO
        int count = 0;

        await Task.Run(() =>
        {
            Parallel.ForEach(imagePaths, (imagePath) =>
            {
                string outputImagePath = Path.ChangeExtension(imagePath, "webp");

                using Image image = Image.Load(imagePath);

                image.Save(outputImagePath, new WebpEncoder() { Quality = 100, NearLossless = true});
                
                File.SetLastWriteTime(outputImagePath, File.GetLastWriteTime(imagePath));
                File.SetCreationTime(outputImagePath, File.GetCreationTime(imagePath));
                File.SetLastAccessTime(outputImagePath, File.GetLastAccessTime(imagePath));

                ConversionProgress = (double)count++ / imagePaths.Length;
            });
        }).ConfigureAwait(false);

        ConversionProgress = 100;

        StatusMessageConvert = LocalizationResource.done; //TODO
        EnableFilesScanButton = true;
    }

    [RelayCommand]
    void ScanForFiles()
    {
        // reset values
        EnableQuickConvertButton = false;
        List<ImageModel> files = new();
        StatusMessageScan = LocalizationResource.loading;
        StatusMessageConvert = LocalizationResource.quick_convert;

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
        StatusMessageScan = $"{files.Count} files detected"; //TODO
        statusMessageConvert = LocalizationResource.quick_convert;
        EnableQuickConvertButton = true;
    }

    [RelayCommand]
    async void GetFileLocation()
    {
        string result = await Application.Current.MainPage.DisplayPromptAsync(LocalizationResource.file_location, "VRC Pictures (Documents folder)", cancel: LocalizationResource.cancel, accept: LocalizationResource.ok, initialValue: settings.SourcePath, keyboard: Keyboard.Text); //TODO

        if (result != null && !string.IsNullOrWhiteSpace(result))
        {
            settings.SourcePath = result;
            settings.SaveData();
        }

    }

    [RelayCommand]
    async void ChangeOutputType()
    {
        const string webp = "WebP";

        string result = await Application.Current.MainPage.DisplayActionSheet("Output file type:", LocalizationResource.cancel, null, webp); //TODO

        switch(result)
        {
            case webp:
                settings.OutputType = OutputTypes.webp;
                settings.SaveData();
                break;
        }
    }
}
