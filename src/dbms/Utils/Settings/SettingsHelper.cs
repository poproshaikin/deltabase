namespace Utils.Settings;

public static class SettingsHelper
{
    public static SettingsCollection Parse(string text)
    {
        string[] splitted = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < splitted.Length; i++)
        {
            splitted[i] = splitted[i].Trim('\n', '\r');
        }

        return new SettingsCollection(splitted.Select(SettingPair.Parse));
    }

    public static string GetKeyFromString(string text, string key)
    {
        SettingsCollection settings = Parse(text);
        return settings.Get(key);
    }
}