using Enums;
using Enums.Network;

namespace Utils.Settings;

public class RequestSettings : SettingsCollection
{
    private const string command_name_key = "command";
    private const string server_name_key = "server";
    private const string database_name_key = "database";
    private const string password_key = "password";
    private const string sql_query_key = "sql";
    private const string client_token_key = "client_token";
    private const string combined_token_key = "combined_token";
    
    public RequestSettings(params IEnumerable<SettingPair> settings) : base(settings)
    {
    }

    public RequestType Type => EnumsStorage.GetCommandType(base.Get(command_name_key));
    public string ServerName => base.Get(server_name_key);
    public string DatabaseName => base.Get(database_name_key);
    public string Password => base.Get(password_key);
    public string SqlQuery => base.Get(sql_query_key);
    public string ClientToken => base.Get(client_token_key);
    public string CombinedToken => base.Get(combined_token_key);
}