using Db.Core;
using Enums.Tcp;
using Server.Tcp;
using Utils;

namespace Server.Core;

/// <summary>
/// Represents a server that listens for client connections and processes their requests.
/// </summary>
public class DltServer
{
    private const int connected_clients_limit = 10;

    private SemaphoreSlim _semaphore;
    
    private FileSystemManager _fs;
    private DltTcpListener _listener;
    
    private Dictionary<DltClient, DltDatabase> _clients;
    
    private DltConnectionConfig _connectionConfig;
    private string _serverName;

    /// <summary>
    /// Initializes a new instance of the <see cref="DltServer"/> class.
    /// </summary>
    /// <param name="cnnConfig">The connection configuration for the server.</param>
    public DltServer(DltConnectionConfig cnnConfig)
    {
        _serverName = cnnConfig.Server;
        _connectionConfig = cnnConfig;
        _fs = new FileSystemManager(_serverName);
        _listener = new DltTcpListener(cnnConfig);
        _semaphore = new SemaphoreSlim(initialCount: 1, maxCount: connected_clients_limit);
        _clients = new Dictionary<DltClient, DltDatabase>();
    }

    /// <summary>
    /// Starts the server and begins listening for client connections.
    /// </summary>
    public void Start()
    {                           
        _listener.Start();
        Logger.LogHeader("Server started successfully");
        
        while (true)
        {
            DltClient client = _listener.AcceptClient();
            
            Logger.Log($"Client accepted: {client.ClientEndPoint}");
            
            ProcessClientAsync(client);
        }
    }

    /// <summary>
    /// Processes a client connection asynchronously.
    /// </summary>
    /// <param name="client">The client to process.</param>
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

    /// <summary>
    /// Processes a request from a client asynchronously.
    /// </summary>
    /// <param name="client">The client that sent the request.</param>
    /// <param name="handler">The handler managing the client's TCP connection.</param>
    /// <param name="request">The request received from the client.</param>
    private async void ProcessRequestAsync(DltClient client, DltTcpHandler handler, TcpRequest request)
    {
        Logger.Log($"request received: \"{request.Message}\"");

        switch (request.CommandType)
        {
            case TcpCommandType.connect:
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
                    handler.Write(ResponseType.ConnectedSuccessfully);
                    return;
                }
                else
                {
                    handler.Write(ResponseType.Unauthorized);
                    handler.Dispose();
                    return;
                }
            }
            case TcpCommandType.sql when !client.Authorized:
            {
                handler.Write(ResponseType.Unauthorized);
                handler.Dispose();
                return;
            }
            case TcpCommandType.sql:
            {
                string sql = request.Data;
                byte[] result = _clients[client].ExecuteRequest(sql);
                
                handler.Write(result);
                return;
            }
            case TcpCommandType.close:
            {
                _clients.Remove(client);
                
                handler.Write(ResponseType.Success);
                handler.Dispose();
                
                return;
            }
        }
    }

    /// <summary>
    /// Initializes a new <see cref="DltServer"/> using the provided connection string.
    /// </summary>
    /// <param name="connectionString">The connection string for the server configuration.</param>
    /// <returns>A new instance of the <see cref="DltServer"/> class.</returns>
    public static DltServer Init(string connectionString)
    {
        var connectionConfig = DltConnectionConfig.Parse(connectionString);
        return new DltServer(connectionConfig);
    }
        
    /// <summary>
    /// Verifies if the request matches the server's connection configuration.
    /// </summary>
    /// <param name="request">The request to verify.</param>
    /// <returns><c>true</c> if the request is authorized; otherwise, <c>false</c>.</returns>
    private bool VerifyRequest(TcpRequest request)
    {
        return request.GetConnectionConfig() == _connectionConfig;
    }
}

