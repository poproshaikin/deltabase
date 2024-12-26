namespace Utils.Settings;

public class SettingPair
{
    private const string multiple_values_separator = "■ ";
    
    public string Key;
    public string Value;

    private SettingPair()
    {
    }

    public SettingPair(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public void Add(string newValue)
    {
        if (Value == null!) 
            Value = newValue;
        else
            Value = Value?.ToString() + multiple_values_separator + newValue.ToString();
    }

    public void Add(Enum newValue) => Add(Convert.ToInt32(newValue).ToString());

    public static SettingPair Parse(string line)
    {
        string[] splitted = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
    
        return new SettingPair(key: splitted[0], value: line[(splitted[0].Length + 1)..]);
    }

    public override string ToString()
    {
        return $"{Key}={Value?.ToString()}\r\n";
    }
}