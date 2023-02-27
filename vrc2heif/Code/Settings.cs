using Newtonsoft.Json;

namespace vrc2heif;

public class Settings
{
    public static string SourcePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VRChat");
    public static int BreakingVersion { get; set; }
    public static bool AutoConvert { get; set; }
    public static bool AutoDelete { get; set; }
    public static bool FunStatsEnabled { get; set; }
    public static FunStats FunStats { get; set; }

    private static string settingsPath = Path.Combine(FileSystem.Current.AppDataDirectory, "settings.json");
    private static string sourcePaths;
    private static object value;

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

    public static void SaveData()
    {
        string settingsJson = JsonConvert.SerializeObject(new Settings(), Formatting.Indented);
        File.WriteAllText(settingsPath, settingsJson);
    }

    public void retrieveData()
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
