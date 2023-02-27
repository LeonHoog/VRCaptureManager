using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using vrc2heif.Model;

namespace vrc2heif.ViewModel;

public partial class ImageViewModel : ObservableObject
{
    private Settings settings;
    public ImageViewModel(Settings settings)
    {
        this.settings = settings;
        this.showQuickConvertButton = false;
        this.statusMessage = "Search for pictures";
    }

    [ObservableProperty]
    bool showQuickConvertButton;

    [ObservableProperty]
    string statusMessage;

    [ObservableProperty]
    ImageModel[] images;

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
    async void OnActionSheetFileLocation()
    {
        settings.retrieveData();
        string result = await Application.Current.MainPage.DisplayPromptAsync("File Location", "VRC Pictures (Documents folder)", initialValue: Settings.SourcePath, keyboard: Keyboard.Text);

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
