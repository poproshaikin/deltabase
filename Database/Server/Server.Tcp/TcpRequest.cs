using Enums.Tcp;
using Utils;

namespace Server.Tcp;

/// <summary>
/// Represents a TCP request containing a command type and associated message data.
/// </summary>
public class TcpRequest
{
    /// <summary>
    /// Gets the command type of the request.
    /// </summary>
    public TcpCommandType CommandType { get; private set; }
    
    /// <summary>
    /// Gets the complete message of the request.
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// Gets the command portion of the message.
    /// </summary>
    public string Command => Message.Split(MESSAGE_SEPARATOR)[0];
    
    /// <summary>
    /// Gets the data portion of the message.
    /// </summary>
    public string Data => Message.Split(MESSAGE_SEPARATOR)[1];
    
    private const string MESSAGE_SEPARATOR = "■ ";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TcpRequest"/> class with the specified message.
    /// Parses the command type from the message.
    /// </summary>
    /// <param name="message">The message received from the TCP client.</param>
    public TcpRequest(string message)
    {
        Message = message;
        var content = Command;
        CommandType = ParseHelper.ParseCommandType(content);
    }
    
    /// <summary>
    /// Gets the connection string from the data portion of the message
    /// and parses it into a <see cref="DltConnectionConfig"/> instance.
    /// </summary>
    /// <returns>A <see cref="DltConnectionConfig"/> instance parsed from the data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the connection string format is invalid.</exception>
    public DltConnectionConfig GetConnectionConfig()
    {
        string connectionString = Data;
        return DltConnectionConfig.Parse(connectionString);
    }
}