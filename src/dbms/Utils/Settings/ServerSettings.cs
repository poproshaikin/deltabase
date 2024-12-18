using Enums.Exceptions;
using Exceptions;

namespace Utils.Settings;

public class ServerSettings : SettingsCollection
{
    private const string name_key = "name";
    private const string port_key = "port";
    private const string password_key = "password";
    private const string transport_protocol_key = "transport_protocol";
    private const string address_key = "address";
    
    public ServerSettings(IEnumerable<SettingPair> settings) : base(settings)
    {
    }

    public string Name => Get(name_key) ?? throw new DbEngineException(ErrorType.MissingServerConfigSetting);
    public string Address => Get(address_key) ?? throw new DbEngineException(ErrorType.MissingServerConfigSetting);
    public ushort Port => ushort.Parse(Get(port_key) ?? throw new DbEngineException(ErrorType.MissingServerConfigSetting));
    public string PasswordHashed => Get(password_key) ?? throw new DbEngineException(ErrorType.MissingServerConfigSetting);
    public string TransportProtocol => Get(transport_protocol_key) ?? throw new DbEngineException(ErrorType.MissingServerConfigSetting);
}