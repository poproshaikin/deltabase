using System.Net.Sockets;
using Server.Core;

namespace Server.Tcp;

public class DltClient : IDisposable
{
    private TcpClient _client;
    private NetworkStream _stream => _client.GetStream();
    public bool IsDisposed { get; private set; }

    public DltConnectionConfig? ConnectionConfig { get; private set; }

    public bool IsConnectionOpened
    {
        // get
        // {
        //     Socket socket = _client.Client;
        //     return socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0;
        // }

        get => _client.Connected;
    }
    public int RequestsAccepted { get; set; }
    public bool Authorized { get; set; }
    public bool RequestPending => _stream.DataAvailable;
    public TcpClient TcpClient => _client;

    public DltClient(TcpClient client, bool authorized = false)
    {
        _client = client;
        RequestsAccepted = 0;
        Authorized = authorized;
    }

    public void Dispose()
    {
        _client.Dispose();
        IsDisposed = true;
    }
}