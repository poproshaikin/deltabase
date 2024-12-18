namespace Utils.Settings;

public class ResponseSettings : SettingsCollection
{
    public ResponseSettings(params IEnumerable<SettingPair> settings) : base(settings)
    {
    }
}