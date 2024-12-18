using System.Net;
using Enums.Exceptions;
using Exceptions;
using Network.Tcp;
using Utils.Settings;

namespace Network;

public interface IHandler
{
    string ReadToEnd();

    void Write(SettingsCollection settings);
    void Write(string text);
    Task WriteAsync(byte b);
    Task WriteAsync(string text);
    Task WriteAsync(SettingsCollection settings);
    
    void AwaitRequest();

#region Factory methods
    
    static IHandler FromProtocol(string protocol, string address, ushort port)
    {
        return protocol switch
        {
            "tcp" => new TcpHandler(address, port),
            
#if CLIENT_LIBRARY
            _ => throw new DltException($"Unsupported protocol: {protocol}"),
#else
            _ => throw new DbEngineException(ErrorType.UnsupportedTransportProtocol)
#endif
        };
    }
    
#endregion
}