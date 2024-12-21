using Enums.Network;
using Exceptions;

namespace Utils.Settings;

public class SettingsCollection
{
    private List<SettingPair> _settings;
    
    public SettingsCollection()
    {
        _settings = [];
    }
    
    public SettingsCollection(params IEnumerable<SettingPair> settings)
    {
        _settings = settings.ToList();
    }

    public string Get(string key)
    {
        if (_settings.Any(s => s.Key == key))
            return _settings.First(s => s.Key == key).Value.ToString();
        
        throw new ArgumentOutOfRangeException(nameof(key), $"The key '{key}' was not found in the settings collection.");
    }

    public string? GetOrDefault(string key)
    {
        return _settings.FirstOrDefault(s => s.Key == key)?.Value;
    }

    public string[] GetMultiple(string key)
    {
        string value = Get(key);
        if (!value.Contains("■ "))
            return [value];
        else
            return value.Split("■ ");
    }

    public void Add(SettingPair setting) => _settings.Add(setting);
    public void Add(string key, Enum value) => Add(key, Convert.ToInt32(value).ToString());
    public void Add(string key, long value) => Add(key, value.ToString());
    public void Add(string key, string value)
    {
        if (_settings.FirstOrDefault(c => c.Key == key) is { } setting)
            setting.Add(value);
        else
            AddNew(key, value);
    }

    private void AddNew(string key, string value)
    {
        _settings.Add(new SettingPair(key, value));
    }

    public void Replace(string key, string value)
    {
        SettingPair? setting = _settings.FirstOrDefault(s => s.Key == key);

        if (setting is null)
            throw new ArgumentOutOfRangeException(nameof(key),
                $"The key '{key}' was not found in the settings collection.");

        setting.Value = value;
    }

    public bool Contains(string key, ResponseType type)
    {
        string[] resultCodes = GetMultiple(key);

        return resultCodes.Contains(((int)type).ToString());
    }
    
    public RequestSettings ToRequestSettings()
    {
        return new RequestSettings(_settings);
    }

    public ServerSettings ToServerSettings()
    {
        return new ServerSettings(_settings);
    }

    public ConnectionSettings ToConnectionSettings()
    {
        return new ConnectionSettings(_settings);
    }
    public ResponseSettings ToResponseSettings()
    {
        return new ResponseSettings(_settings);
    }

    public DatabaseSettings ToDatabaseSettings()
    {
        return new DatabaseSettings(_settings);
    }

    public override string ToString()
    {
        return _settings.Aggregate(string.Empty, (current, setting) => current + setting.ToString());
    }
}

