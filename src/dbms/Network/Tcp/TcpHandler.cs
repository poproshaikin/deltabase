using System.Net.Sockets;
using Utils;
using Utils.Settings;

namespace Network.Tcp;

internal class TcpHandler : IHandler
{
    private TcpClient _client;
    
    private NetworkStream _stream => _client.GetStream();

    internal TcpHandler(string address, ushort port) : this(new TcpClient(address, port))
    {
    }
    
    internal TcpHandler(TcpClient client)
    {
        _client = client;
    }

    public string ReadToEnd()
    {
        byte[] buffer = new byte[_client.Available];
        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
        return ConvertHelper.GetString(buffer[0..bytesRead]);
    }

    public void Write(SettingsCollection settings) => Write(settings.ToString());
    public void Write(string message)
    {
        byte[] buffer = ConvertHelper.GetBytes(message);
        _stream.Write(buffer, 0, buffer.Length);
        _stream.Flush(); 
    }

    public async Task WriteAsync(SettingsCollection settings) => await WriteAsync(settings.ToString());
    public async Task WriteAsync(string message)
    {
        byte[] buffer = ConvertHelper.GetBytes(message);
        await _stream.WriteAsync(buffer, 0, buffer.Length);
        await _stream.FlushAsync();
    }

    public Task WriteAsync(byte b)
    {
        _stream.WriteByte(b);
        return Task.CompletedTask;
    }

    public void AwaitRequest()
    {
        while (_client.Available == 0)
        {
            Thread.Sleep(100);
        }
    }
}