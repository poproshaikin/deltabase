namespace Server.Core;

public class DltConnectionConfig
{
    private const string TEMPLATE = "server;database;ip;port;password";

    public string Server;
    public string? Database;
    public string Address;
    public ushort Port;
    public string Password;

    private DltConnectionConfig(string server, string address, ushort port, string password)
    {
        Server = server;
        Address = address;
        Port = port;
        Password = password;
    }
    private DltConnectionConfig(string server, string database, string address, ushort port, string password)
    {
        Server = server;
        Database = database;
        Address = address;
        Port = port;
        Password = password;
    }
    
    /// <exception cref="InvalidOperationException">Throws if connection string format is invalid</exception>
    public static DltConnectionConfig Parse(string cnnString)
    {
        ArgumentNullException.ThrowIfNull(cnnString, nameof(cnnString));

        string[] splitted = cnnString.Split(';');
        return Parse(splitted);
    }

    /// <exception cref="InvalidOperationException">Throws if connection string format is invalid</exception>
    public static DltConnectionConfig Parse(string[] confLines)
    {
        return ValidateWithTemplate(confLines);
    }

    /// <summary>
    /// Validates the connection string lines array with connection config parsing template and returns the new DltConnectionConfig instance
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if connection string format is invalid</exception>
    private static DltConnectionConfig ValidateWithTemplate(string[] lines)
    {
        return lines.Length switch
        {
            4 => new DltConnectionConfig(lines[0], lines[1], Convert.ToUInt16(lines[2]), lines[3]),
            5 => new DltConnectionConfig(lines[0], lines[1], lines[2], Convert.ToUInt16(lines[3]), lines[4]),

            _ => throw new InvalidOperationException("Incorrect connection string format")
        };
    }

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

    public static bool operator !=(DltConnectionConfig? left, DltConnectionConfig? right)
    {
        return !(left == right);
    }
    
    protected bool Equals(DltConnectionConfig other)
    {
        return Server == other.Server && Database == other.Database && Address == other.Address && Port == other.Port && Password == other.Password;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DltConnectionConfig)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Server, Database, Address, Port, Password);
    }
}