using System.Net.Sockets;

namespace Server.Tcp;

/// <summary>
/// Represents a client connected to the server via TCP.
/// </summary>
public class DltClient : IDisposable
{
    private TcpClient _client;
    
    /// <summary>
    /// Gets the network stream for the client.
    /// </summary>
    private NetworkStream _stream => _client.GetStream();
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets or sets the connection configuration for the client.
    /// </summary>
    public DltConnectionConfig? ConnectionConfig { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the connection to the server is still open.
    /// </summary>
    public bool IsConnectionOpened => _client.Connected;
    
    /// <summary>
    /// Gets or sets the number of requests that the server has accepted from the client.
    /// </summary>
    public int RequestsAccepted { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the client is authorized to interact with the server.
    /// </summary>
    public bool Authorized { get; set; }
    
    /// <summary>
    /// Gets a value indicating whether there is a pending request from the client.
    /// </summary>
    public bool RequestPending => _stream.DataAvailable;
    
    /// <summary>
    /// Gets the underlying <see cref="TcpClient"/> for the client.
    /// </summary>
    public TcpClient TcpClient => _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="DltClient"/> class.
    /// </summary>
    /// <param name="client">The TCP client instance representing the connection to the server.</param>
    /// <param name="authorized">Indicates whether the client is authorized.</param>
    public DltClient(TcpClient client, bool authorized = false)
    {
        _client = client;
        RequestsAccepted = 0;
        Authorized = authorized;
    }

    /// <summary>
    /// Releases the resources used by the <see cref="DltClient"/> instance.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        IsDisposed = true;
    }
}