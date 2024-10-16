using System.Net.Sockets;
using System.Text;
using Utils;

namespace Deltabase.Driver;

internal class TcpHelper
{
    private TcpClient _client;

    private string _address;
    private int _port;
    
    public TcpClient Client => _client;
    public NetworkStream Stream => _client.GetStream();
    
    public TcpHelper(string address, int port)
    {
        _address = address;
        _port = port;
    }

    public void Connect()
    {
        _client = new TcpClient(_address, _port);
    }

    public void Write(string msg)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(msg);

        Stream.Write(buffer);
        Stream.Flush();
    }

    public string Read()
    {
        AwaitResponse();
        
        byte[] buffer = new byte[Client.Available];
        _ = Stream.Read(buffer, 0, buffer.Length);

        return ConvertHelper.GetString(buffer);
    }

    public void AwaitResponse()
    {
        while (!Stream.DataAvailable)
        {
            Thread.Sleep(100);
        }
    }
}