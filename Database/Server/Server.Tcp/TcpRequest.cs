using Enums.Tcp;
using Server.Core;
using Utils;

namespace Server.Tcp;

public class TcpRequest
{
    public TcpCommandType CommandType { get; private set; }
    public string Message { get; private set; }

    public string Command => Message.Split(MESSAGE_SEPARATOR)[0];
    public string Data => Message.Split(MESSAGE_SEPARATOR)[1];
    
    private const string MESSAGE_SEPARATOR = "■ ";
    
    public TcpRequest(string message)
    {
        Message = message;
        var content = Command;
        CommandType = ParseHelper.ParseCommandType(content);
    }
    
    /// <summary>
    /// Gets the connection string and parses it to a <see cref="DltConnectionConfig"/> instance
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if connection string format is invalid</exception>
    public DltConnectionConfig GetConnectionConfig()
    {
        string connectionString = Data;
        return DltConnectionConfig.Parse(connectionString);
    }
}