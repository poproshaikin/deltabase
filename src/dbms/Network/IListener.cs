using System.Net;
using System.Net.Sockets;
using Enums.Exceptions;
using Exceptions;
using Network.Tcp;

namespace Network;

public interface IListener
{
    EndPoint ListeningEndPoint { get; }
    
    void Start();
    IConnection AcceptConnection();
    
    #region Factory methods
    
    // static IListener FromProtocol(string transportProtocol, string ip, ushort port)
    // {
    //     return transportProtocol switch
    //     {
    //         "tcp" => new TcpListener(ip, port),
    //         _ => throw new DbEngineException(ErrorType.UnsupportedTransportProtocol)
    //     };
    // }
    //
    // static IListener FromProtocol(string transportProtocol, IPAddress ip, ushort port)
    // {
    //     return transportProtocol switch
    //     {
    //         "tcp" => new TcpListener(ip, port),
    //         _ => throw new DbEngineException(ErrorType.UnsupportedTransportProtocol)
    //     };
    // }
    
    #endregion

}