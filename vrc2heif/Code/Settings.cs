using Newtonsoft.Json;

namespace vrc2heif;

public class Settings
{
    public string SourcePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VRChat");
    public int BreakingVersion { get; set; }
    public bool AutoConvert { get; set; }
    public bool AutoDelete { get; set; }
    public bool FunStatsEnabled { get; set; }
    public FunStats FunStats { get; set; }
    private readonly string settingsPath = Path.Combine(FileSystem.Current.AppDataDirectory, "settings.json");

    public Settings()
    {
        SourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VRChat");
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

    public void RetrieveData()
    {
        if (File.Exists(settingsPath))
        {
            string settingsJson = File.ReadAllText(settingsPath);
            Settings settings = JsonConvert.DeserializeObject<Settings>(settingsJson);
        }
    }
}

public class FunStats
{
    public int Counter { get; set; }
    public int DataSaved { get; set; }
}
