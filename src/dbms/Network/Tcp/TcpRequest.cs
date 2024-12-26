using System.Net;
using System.Net.Sockets;
using Enums;
using Enums.Network;
using Utils.Settings;

namespace Network.Tcp;

internal class TcpRequest : IRequest
{
    private TcpClient _sender;
    
    public string Content { get; set; }
    public RequestType Type { get; set; }
    public EndPoint RemoteEndPoint => _sender.Client.RemoteEndPoint;
    internal TcpClient Sender => _sender;

    internal TcpRequest(TcpClient sender, string content)
    {
        Content = content;
        Type = EnumsStorage.GetCommandType(SettingsHelper.GetKeyFromString(content, "command"));
        _sender = sender;
    }
    
    public IHandler GetHandler()
    {
        return new TcpHandler(_sender);
    }
}