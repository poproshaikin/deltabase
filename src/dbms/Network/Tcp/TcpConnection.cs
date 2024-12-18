using System.Net.Sockets;

namespace Network.Tcp;

internal class TcpConnection : IConnection
{
    private readonly TcpClient _client;
    private readonly Socket _socket;

    public Guid Id { get; }
    public DateTime LastActivity { get; private set; }

    public bool IsConnected
    {
        get
        {
            try
            {
                if (_client != null! && _client.Client != null! && _client.Client.Connected)
                {
                    /* pear to the documentation on Poll:
                     * When passing SelectMode.SelectRead as a parameter to the Poll method it will return
                     * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                     * -or- true if data is available for reading;
                     * -or- true if the connection has been closed, reset, or terminated;
                     * otherwise, returns false
                     */

                    // Detect if client disconnected
                    if (_client.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (_client.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            // Client disconnected
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public TcpClient Client
    {
        get
        {
            LastActivity = DateTime.Now;
            return _client;
        }
        private init
        {
            _client = value;
            _socket = value.Client;
        }
    }
    
    internal Socket Socket => _socket;

    private TcpConnection()
    {
        Id = Guid.NewGuid();
        LastActivity = DateTime.Now;
    }
    
    public TcpConnection(TcpClient client) : this()
    {
        Client = client;
    }

    public void Dispose() => Close();
    
    public void Close()
    {
        _client.Close();
    }
    
    public IHandler GetHandler() => new TcpHandler(_client);
}