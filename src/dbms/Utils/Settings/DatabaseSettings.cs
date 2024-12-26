namespace Utils.Settings;

public class DatabaseSettings : SettingsCollection
{
    public DatabaseSettings(params IEnumerable<SettingPair> settings) : base(settings)
    {
    }

    public string? Encoding => GetOrDefault("encoding");
}