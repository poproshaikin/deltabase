using System.Net;
using Enums.Network;

namespace Network;

public interface IRequest
{
    EndPoint RemoteEndPoint { get; }
    string Content { get; }
    RequestType Type { get; }

    IHandler GetHandler();
}