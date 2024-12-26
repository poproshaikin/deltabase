using System.Net;
using Enums.Exceptions;
using Exceptions;
using Network.Tcp;

namespace Network;

public interface IConnectionsPool : IDisposable
{
    IConnection AwaitPendingConnection();
    void WithListener(string address, int port);
    EndPoint? ListeningEndPoint { get; }

    void Add(IConnection connection);
    void Remove(IConnection connection);
    void RemoveByRequest(IRequest request);
    
    void Stop(bool closeConnections);
    
    static IConnectionsPool FromProtocol(string protocol)
    {
        return protocol switch
        {
            "tcp" => new TcpConnectionsPool(),
            _ => throw new DbEngineException(ErrorType.UnsupportedTransportProtocol)
        };
    }
}