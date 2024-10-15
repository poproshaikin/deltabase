namespace Server.Tcp;

/// <summary>
/// Represents the configuration for a connection to the server, including server, database, and network information.
/// </summary>
public class DltConnectionConfig
{
    private const string TEMPLATE = "server;database;ip;port;password";

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string Server;

    /// <summary>
    /// Gets or sets the database name. This can be null if no database is specified.
    /// </summary>
    public string? Database;

    /// <summary>
    /// Gets or sets the server address (IP or domain).
    /// </summary>
    public string Address;

    /// <summary>
    /// Gets or sets the port number used for the connection.
    /// </summary>
    public ushort Port;

    /// <summary>
    /// Gets or sets the password for the connection.
    /// </summary>
    public string Password;

    /// <summary>
    /// Initializes a new instance of the <see cref="DltConnectionConfig"/> class with the specified server, address, port, and password.
    /// </summary>
    /// <param name="server">The server name.</param>
    /// <param name="address">The server address (IP or domain).</param>
    /// <param name="port">The port number.</param>
    /// <param name="password">The password for the connection.</param>
    private DltConnectionConfig(string server, string address, ushort port, string password)
    {
        Server = server;
        Address = address;
        Port = port;
        Password = password;
    }

    private DltConnectionConfig(string server, ushort port, string password)
    {
        Server = server;
        Port = port;
        Password = password;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DltConnectionConfig"/> class with the specified server, database, address, port, and password.
    /// </summary>
    /// <param name="server">The server name.</param>
    /// <param name="database">The database name.</param>
    /// <param name="address">The server address (IP or domain).</param>
    /// <param name="port">The port number.</param>
    /// <param name="password">The password for the connection.</param>
    private DltConnectionConfig(string server, string database, string address, ushort port, string password)
    {
        Server = server;
        Database = database;
        Address = address;
        Port = port;
        Password = password;
    }
    
    /// <summary>
    /// Parses a connection string into a <see cref="DltConnectionConfig"/> instance.
    /// </summary>
    /// <param name="cnnString">The connection string to parse.</param>
    /// <returns>A new <see cref="DltConnectionConfig"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the connection string format is invalid.</exception>
    public static DltConnectionConfig Parse(string cnnString)
    {
        ArgumentNullException.ThrowIfNull(cnnString, nameof(cnnString));

        string[] splitted = cnnString.Split(';');
        return Parse(splitted);
    }

    /// <summary>
    /// Parses an array of connection string components into a <see cref="DltConnectionConfig"/> instance.
    /// </summary>
    /// <param name="confLines">The array of connection string components.</param>
    /// <returns>A new <see cref="DltConnectionConfig"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the connection string format is invalid.</exception>
    public static DltConnectionConfig Parse(string[] confLines)
    {
        return ValidateWithTemplate(confLines);
    }

    /// <summary>
    /// Validates the connection string components against the template and returns a new <see cref="DltConnectionConfig"/> instance.
    /// </summary>
    /// <param name="lines">The array of connection string components.</param>
    /// <returns>A new <see cref="DltConnectionConfig"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the connection string format is invalid.</exception>
    private static DltConnectionConfig ValidateWithTemplate(string[] lines)
    {
        return lines.Length switch
        {
            3 => new DltConnectionConfig(lines[0], Convert.ToUInt16(lines[1]), lines[2]),
            4 => new DltConnectionConfig(lines[0], lines[1], Convert.ToUInt16(lines[2]), lines[3]),
            5 => new DltConnectionConfig(lines[0], lines[1], lines[2], Convert.ToUInt16(lines[3]), lines[4]),

            _ => throw new InvalidOperationException("Incorrect connection string format")
        };
    }

    /// <summary>
    /// Compares two <see cref="DltConnectionConfig"/> instances for equality.
    /// </summary>
    /// <param name="left">The first <see cref="DltConnectionConfig"/>.</param>
    /// <param name="right">The second <see cref="DltConnectionConfig"/>.</param>
    /// <returns>True if both instances are equal; otherwise, false.</returns>
    public static bool operator ==(DltConnectionConfig? left, DltConnectionConfig? right)
    {
        if (left is null && right is null)
            return true;
        
        if (left is null || right is null)
            return false;

        if (left.Database == null || right.Database == null)
        {
            return left.Server   == right.Server   &&
                   left.Password == right.Password &&
                   left.Address  == right.Address  &&
                   left.Port     == right.Port;
        }

        return left.Server   == right.Server   &&
               left.Database == right.Database &&
               left.Password == right.Password &&
               left.Address  == right.Address  &&
               left.Port     == right.Port;
    }

    /// <summary>
    /// Compares two <see cref="DltConnectionConfig"/> instances for inequality.
    /// </summary>
    /// <param name="left">The first <see cref="DltConnectionConfig"/>.</param>
    /// <param name="right">The second <see cref="DltConnectionConfig"/>.</param>
    /// <returns>True if both instances are not equal; otherwise, false.</returns>
    public static bool operator !=(DltConnectionConfig? left, DltConnectionConfig? right)
    {
        return !(left == right);
    }
    
    /// <summary>
    /// Determines whether the specified <see cref="DltConnectionConfig"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="DltConnectionConfig"/> to compare with the current instance.</param>
    /// <returns>True if both instances are equal; otherwise, false.</returns>
    protected bool Equals(DltConnectionConfig other)
    {
        return Server == other.Server && Database == other.Database && Address == other.Address && Port == other.Port && Password == other.Password;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DltConnectionConfig)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Server, Database, Address, Port, Password);
    }
}