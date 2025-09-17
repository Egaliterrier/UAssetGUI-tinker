using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.IO;
using UAssetAPI.UnrealTypes;
using UAssetManager.Resources;
using UAssetManager.Resources.Themes;

namespace UAssetManager.Models;
public partial class UAGConfigData : ObservableObject, ICloneable
{
    [ObservableProperty] EngineVersion _preferredVersion;
    [ObservableProperty] string _preferredMappings = string.Empty;
    [ObservableProperty] ThemeType _theme = ThemeType.Light;
    [ObservableProperty] ELanguage _language = ELanguage.English;
    [ObservableProperty] string _mapStructTypeOverride = string.Empty;
    [ObservableProperty] bool _changeValuesOnScroll;
    [ObservableProperty] bool _enableDynamicTree;
    [ObservableProperty] bool _enableDiscordRPC;
    [ObservableProperty] bool _enableBak;
    [ObservableProperty] bool _restoreSize;
    [ObservableProperty] bool _enableUpdateNotice = true;
    [ObservableProperty] bool _enablePrettyBytecode;
    [ObservableProperty] bool _useOuterIndexTreeMode;
    [ObservableProperty] int _startupWidth = 1000;
    [ObservableProperty] int _startupHeight = 700;
    [ObservableProperty] int _customSerializationFlags;

    partial void OnLanguageChanged(ELanguage value)
    {
        if (StringHelper.Current != null)
        {
            StringHelper.Current.Language = value;
        }
    }

    public object Clone() => MemberwiseClone();
}

public static class UAGConfig
{
    private static UAGConfigData? data;
    public static UAGConfigData Data
    {
        set => data = value;
        get
        {
            if (data is null) Load();
            return data ?? throw new NotSupportedException();
        }
    }

    private static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xylia", "config.json");

    public static readonly string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xylia");
    public static readonly string StagingFolder = Path.Combine(ConfigFolder, "Staging");
    public static readonly string ExtractedFolder = Path.Combine(ConfigFolder, "Extracted");
    public static readonly string MappingsFolder = Path.Combine(ConfigFolder, "Mappings");
    public static readonly bool DifferentStagingPerPak = true;

    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string jsonContent = File.ReadAllText(ConfigPath);
                Data = JsonConvert.DeserializeObject<UAGConfigData>(jsonContent)!;
            }
            else
            {
                Data = new UAGConfigData();
                Save(); // Create default config file
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading config file: {ex.Message}");
            Data = new UAGConfigData();
        }
    }

    public static void Save()
    {
        try
        {
            string configDir = Path.GetDirectoryName(ConfigPath)!;
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

            string jsonContent = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(ConfigPath, jsonContent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving config file: {ex.Message}");
        }
    }

    public static void ResetToDefaults()
    {
        // keep language setting
        var language = Data.Language;
        Data = new UAGConfigData { Language = language };
        Save();
    }
}