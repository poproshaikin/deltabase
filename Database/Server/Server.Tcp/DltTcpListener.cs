using System.Net;
using System.Net.Sockets;

namespace Server.Tcp;

/// <summary>
/// A TCP listener that listens for incoming client connections using the configuration provided in <see cref="DltConnectionConfig"/>.
/// </summary>
public class DltTcpListener : DltTcpService
{
    private TcpListener _listener;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DltTcpListener"/> class, using the specified connection configuration.
    /// </summary>
    /// <param name="cnnConfig">The connection configuration used to initialize the listener, which includes the IP address and port.</param>
    public DltTcpListener(DltConnectionConfig cnnConfig) : base(cnnConfig)
    {
        _listener = new TcpListener(IPAddress.Parse(cnnConfig.Address), cnnConfig.Port);
    }
    
    /// <summary>
    /// Starts the TCP listener, allowing it to begin accepting incoming client connections.
    /// </summary>
    public void Start()
    {
        _listener.Start();
    }
    
    /// <summary>
    /// Accepts an incoming client connection and returns a <see cref="DltClient"/> instance representing the connected client.
    /// </summary>
    /// <returns>A <see cref="DltClient"/> instance that wraps the accepted <see cref="TcpClient"/> connection.</returns>
    public DltClient AcceptClient()
    {
        TcpClient acceptedClient = _listener.AcceptTcpClient();
        return new DltClient(acceptedClient);
    }
}