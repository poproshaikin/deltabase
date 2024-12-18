namespace Utils.Settings;

public class ConnectionSettings : SettingsCollection
{
    private const string server_name_key = "server";
    private const string database_name_key = "database";
    private const string ip_address_key = "address";
    private const string port_key = "port";
    private const string password_key = "password";

    public ConnectionSettings() : base()
    {
    }

    public ConnectionSettings(params IEnumerable<SettingPair> settings) : base(settings)
    {
    }

    public string Server => base.Get(server_name_key);
    public string Database => base.Get(database_name_key);
    public string IpAddress => base.Get(ip_address_key);
    public int Port => int.Parse(base.Get(port_key));
    public string Password => base.Get(password_key);

    // private const string TEMPLATE = "server;database;ip;port;password";
    //
    // public string Server;
    // public string? Database;
    // public string Address;
    // public ushort Port;
    // public string Password;
    //
    // private ConnectionSettings(string server, string address, ushort port, string password)
    // {
    //     Server = server;
    //     Address = address;
    //     Port = port;
    //     Password = password;
    // }
    //
    // private ConnectionSettings(string server, ushort port, string password)
    // {
    //     Server = server;
    //     Port = port;
    //     Password = password;
    // }
    //
    // private ConnectionSettings(string server, string database, string address, ushort port, string password)
    // {
    //     Server = server;
    //     Database = database;
    //     Address = address;
    //     Port = port;
    //     Password = password;
    // }
    //
    // public static ConnectionSettings Parse(string cnnString)
    // {
    //     ArgumentNullException.ThrowIfNull(cnnString, nameof(cnnString));
    //
    //     string[] splitted = cnnString.Split(';');
    //     return Parse(splitted);
    // }
    //
    // public static ConnectionSettings Parse(string[] confLines)
    // {
    //     return ValidateWithTemplate(confLines);
    // }
    //
    // private static ConnectionSettings ValidateWithTemplate(string[] lines)
    // {
    //     return lines.Length switch
    //     {
    //         3 => new ConnectionSettings(lines[0], Convert.ToUInt16(lines[1]), lines[2]),
    //         4 => new ConnectionSettings(lines[0], lines[1], Convert.ToUInt16(lines[2]), lines[3]),
    //         5 => new ConnectionSettings(lines[0], lines[1], lines[2], Convert.ToUInt16(lines[3]), lines[4]),
    //
    //         _ => throw new InvalidOperationException("Incorrect connection string format")
    //     };
    // }
    //
    // public static bool operator ==(ConnectionSettings? left, ConnectionSettings? right)
    // {
    //     if (left is null && right is null)
    //         return true;
    //     
    //     if (left is null || right is null)
    //         return false;
    //
    //     if (left.Database == null || right.Database == null)
    //     {
    //         return left.Server   == right.Server   &&
    //                left.Password == right.Password &&
    //                left.Address  == right.Address  &&
    //                left.Port     == right.Port;
    //     }
    //
    //     return left.Server   == right.Server   &&
    //            left.Database == right.Database &&
    //            left.Password == right.Password &&
    //            left.Address  == right.Address  &&
    //            left.Port     == right.Port;
    // }
    //
    // /// <summary>
    // /// Compares two <see cref="ConnectionSettings"/> instances for inequality.
    // /// </summary>
    // /// <param name="left">The first <see cref="ConnectionSettings"/>.</param>
    // /// <param name="right">The second <see cref="ConnectionSettings"/>.</param>
    // /// <returns>True if both instances are not equal; otherwise, false.</returns>
    // public static bool operator !=(ConnectionSettings? left, ConnectionSettings? right)
    // {
    //     return !(left == right);
    // }
    //
    // /// <summary>
    // /// Determines whether the specified <see cref="ConnectionSettings"/> is equal to the current instance.
    // /// </summary>
    // /// <param name="other">The <see cref="ConnectionSettings"/> to compare with the current instance.</param>
    // /// <returns>True if both instances are equal; otherwise, false.</returns>
    // protected bool Equals(ConnectionSettings other)
    // {
    //     return Server == other.Server && Database == other.Database && Address == other.Address && Port == other.Port && Password == other.Password;
    // }
    //
    // /// <inheritdoc />
    // public override bool Equals(object? obj)
    // {
    //     if (ReferenceEquals(null, obj)) return false;
    //     if (ReferenceEquals(this, obj)) return true;
    //     if (obj.GetType() != this.GetType()) return false;
    //     return Equals((ConnectionSettings)obj);
    // }
    //
    // /// <inheritdoc />
    // public override int GetHashCode()
    // {
    //     return HashCode.Combine(Server, Database, Address, Port, Password);
    // }
}