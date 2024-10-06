using System.Net;
using System.Net.Sockets;
using Server.Core;

namespace Server.Tcp;

public class DltTcpListener : DltTcpService
{
    private TcpListener _listener;
    
    public DltTcpListener(DltConnectionConfig cnnConfig) : base(cnnConfig)
    {
        _listener = new TcpListener(IPAddress.Parse(cnnConfig.Address), cnnConfig.Port);
    }
    
    public void Start()
    {
        _listener.Start();
    }
    
    public DltClient AcceptClient()
    {
        TcpClient acceptedClient = _listener.AcceptTcpClient();
        return new DltClient(acceptedClient);
    }
}