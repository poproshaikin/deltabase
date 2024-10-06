using Db.Core;
using Enums.Tcp;
using Server.Tcp;
using Utils;

namespace Server.Core;

public class DltServer
{
    private const int connected_clients_limit = 10;

    private SemaphoreSlim _semaphore;
    
    private FileSystemManager _fs;
    private DltTcpListener _listener;
    
    private Dictionary<DltClient, DltDatabase> _clients;
    
    private DltConnectionConfig _connectionConfig;
    private string _serverName;
    
    private const string REDIRECTION_MESSAGE = "redirection";
    private const string DATABASE_NULL_MESSAGE = "database name was null";
    private const string UNAUTHORIZED_MESSAGE = "unauthorized";

    public DltServer(DltConnectionConfig cnnConfig)
    {
        _serverName = cnnConfig.Server;
        _connectionConfig = cnnConfig;
        _fs = new FileSystemManager(_serverName);
        _listener = new DltTcpListener(cnnConfig);
        _semaphore = new SemaphoreSlim(connected_clients_limit);
        _clients = new Dictionary<DltClient, DltDatabase>();
    }

    public void Start()
    {
        _listener.Start();
        Logger.LogHeader("Server started successfully");
        
        while (true)
        {
            DltClient client = _listener.AcceptClient();
            
            ProcessClientAsync(client);
        }
    }

    private async void ProcessClientAsync(DltClient client)
    {
        await _semaphore.WaitAsync(); 
        
        using DltTcpHandler handler = new(client.TcpClient);
        
        while (!handler.IsDisposed)
        {
            TcpRequest request = await handler.AwaitRequestAsync();

            ProcessRequestAsync(client, handler, request);
        }

        _semaphore.Release();
        
        Logger.Log($"Connection with host {client.TcpClient.Client.RemoteEndPoint} lost");
    }

    private async void ProcessRequestAsync(DltClient client, DltTcpHandler handler, TcpRequest request)
    {
        Logger.Log($"request received: \"{request.Message}\"");
             
        if (request.CommandType == TcpCommandType.connect)
        {
            bool authorized = VerifyRequest(request);

            if (authorized)
            {
                client.Authorized = true;
                client.RequestsAccepted++;

                _clients[client] = DltDatabaseInitializer.Init(request.GetConnectionConfig().Database!, _fs) ?? 
                                   throw new InvalidOperationException(
                                       $"The database named \"{request.GetConnectionConfig().Database}\" wasn't found");

                handler.ConnectionConfig = request.GetConnectionConfig();
                handler.Write(TcpResponseType.ConnectedSuccessfully);
                return;
            }
            else
            {
                handler.Write(TcpResponseType.Unauthorized);
                handler.Dispose();
                return;
            }
        }
        else if (request.CommandType == TcpCommandType.sql)
        {
            if (!client.Authorized)
            {
                handler.Write(TcpResponseType.Unauthorized);
                handler.Dispose();
                return;
            }

            string sql = request.Data;
            byte[] result = _clients[client].ExecuteRequest(sql);
                
            handler.Write(result);
            return;
        }
    }

    public static DltServer Init(string connectionString)
    {
        var connectionConfig = DltConnectionConfig.Parse(connectionString);
        return new DltServer(connectionConfig);
    }

    private bool VerifyRequest(TcpRequest request)
    {
        return request.GetConnectionConfig() == _connectionConfig;
    }
}

