using Newtonsoft.Json;

namespace vrcapturemanager;

public class Settings
{
    public string SourcePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VRChat");
    public OutputTypes OutputType { get; set; }
    public int BreakingVersion { get; set; }
    public bool AutoConvert { get; set; }
    public bool AutoDelete { get; set; }
    public bool FunStatsEnabled { get; set; }
    public FunStats FunStats { get; set; }
    private readonly string settingsPath = Path.Combine(FileSystem.Current.AppDataDirectory, "settings.json");

    public Settings()
    {
        SourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VRChat");
        OutputType = OutputTypes.webp;
        BreakingVersion = 0;
        AutoConvert = false;
        AutoDelete = false;
        FunStatsEnabled = true;
        FunStats = new FunStats
        {
            Counter = 0,
            DataSaved = 0
        };

        if (!File.Exists(settingsPath))
            SaveData();
    }

    public void SaveData()
    {
        string settingsJson = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(settingsPath, settingsJson);
    }

    public Settings RetrieveData()
    {
        if (File.Exists(settingsPath))
        {
            string settingsJson = File.ReadAllText(settingsPath);
            Settings settings = JsonConvert.DeserializeObject<Settings>(settingsJson);
            return settings;
        }
        return null;
    }
}

public enum OutputTypes
{
    webp,
    heif
}

public class FunStats
{
    public int Counter { get; set; }
    public int DataSaved { get; set; }
}
