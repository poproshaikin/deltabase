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
    
    public DltServer(string serverName)
    {
        _fs = new FileSystemManager(serverName);
        DltConnectionConfig config = DltConnectionConfig.Parse(_fs.ReadServerConnectionConfigFile());
        _listener = new DltTcpListener(config);
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
                await ExecuteConnectAsync(client, handler, request);
                return;
            }
            case TcpCommandType.sql when !client.Authorized:
            {
                await ExecuteSqlWhenNotAuthorizedAsync(client, handler, request);
                return;
            }
            case TcpCommandType.sql:
            {
                await ExecuteSqlAsync(client, handler, request);
                return;
            }
            case TcpCommandType.createdatabase:
            {
                await ExecuteCreateDatabaseAsync(client, handler, request);
                return;
            }
            case TcpCommandType.close:
            {
                await ExecuteCloseAsync(client, handler, request);
                return;
            }
        }
    }

    /// <summary>
    /// Asynchronously handles the connection process, authorizing the client and initializing the database connection if authorized.
    /// </summary>
    /// <param name="client">The <see cref="DltClient"/> attempting to connect.</param>
    /// <param name="handler">The <see cref="DltTcpHandler"/> responsible for communication.</param>
    /// <param name="request">The <see cref="TcpRequest"/> containing connection details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified database is not found.</exception>
    private async Task ExecuteConnectAsync(DltClient client, DltTcpHandler handler, TcpRequest request)
    {
        // TODO реализовать способы авторизации
        bool authorized = true;
                
        if (authorized)
        {
            client.Authorized = true;
            client.RequestsAccepted++;

            _clients[client] = DltDatabaseInitializer.Init(request.GetConnectionConfig().Database!, _fs) ?? 
                               throw new InvalidOperationException(
                                   $"The database named \"{request.GetConnectionConfig().Database}\" wasn't found");

            handler.ConnectionConfig = request.GetConnectionConfig();
            await handler.WriteAsync(ResponseType.ConnectedSuccessfully);
        }
        else
        {
            await handler.WriteAsync(ResponseType.Unauthorized);
            handler.Dispose();
        }
    }

    /// <summary>
    /// Asynchronously handles SQL requests when the client is not authorized, responding with an unauthorized message.
    /// </summary>
    /// <param name="client">The <see cref="DltClient"/> attempting to execute the request.</param>
    /// <param name="handler">The <see cref="DltTcpHandler"/> responsible for communication.</param>
    /// <param name="request">The <see cref="TcpRequest"/> containing the SQL request details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteSqlWhenNotAuthorizedAsync(DltClient client, DltTcpHandler handler, TcpRequest request)
    {
        await handler.WriteAsync(ResponseType.Unauthorized);
        handler.Dispose();
    }

    
    /// <summary>
    /// Asynchronously handles SQL request execution, running the provided SQL command and returning the result.
    /// </summary>
    /// <param name="client">The <see cref="DltClient"/> executing the SQL request.</param>
    /// <param name="handler">The <see cref="DltTcpHandler"/> responsible for communication.</param>
    /// <param name="request">The <see cref="TcpRequest"/> containing the SQL command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteSqlAsync(DltClient client, DltTcpHandler handler, TcpRequest request)
    {
        string sql = request.Data;
        byte[] result = _clients[client].ExecuteRequest(sql);
                
        await handler.WriteAsync(result);
    }
    
    /// <summary>
    /// Asynchronously creates a new database, initializing its folders and configuration files.
    /// </summary>
    /// <param name="client">The <see cref="DltClient"/> requesting database creation.</param>
    /// <param name="handler">The <see cref="DltTcpHandler"/> responsible for communication.</param>
    /// <param name="request">The <see cref="TcpRequest"/> containing the database name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteCreateDatabaseAsync(DltClient client, DltTcpHandler handler, TcpRequest request)
    {
        string dbName = request.Data;
        
        _fs.CreateDbFolder(dbName);
        await _fs.CreateDbConfigFileAsync(dbName);
        _fs.CreateRecordsFolder(dbName);
        
        await handler.WriteAsync(ResponseType.Success);
    }
    
    /// <summary>
    /// Asynchronously handles the closure of the connection, removing the client and responding with a success message.
    /// </summary>
    /// <param name="client">The <see cref="DltClient"/> to be disconnected.</param>
    /// <param name="handler">The <see cref="DltTcpHandler"/> responsible for communication.</param>
    /// <param name="request">The <see cref="TcpRequest"/> related to the closure request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteCloseAsync(DltClient client, DltTcpHandler handler, TcpRequest request)
    {
        _clients.Remove(client);

        await handler.WriteAsync(ResponseType.Success);
        handler.Dispose();
    }

    /// <summary>
    /// Creates a new instance of <see cref="DltServer"/>, initializing necessary server folders and configuration files.
    /// </summary>
    /// <param name="serverName">The name of the server to be created.</param>
    /// <param name="password">The password to be used for server authentication.</param>
    /// <returns>A new instance of <see cref="DltServer"/>.</returns>
    public static DltServer Create(string serverName, ushort port, string password)
    {
        DltServer server = new(serverName);
        server._fs.CreateServerFolder();
        server._fs.CreateServerConfigFile(port, password);
        server._fs.CreateDbsFile();
        server._fs.CreateDbInServerFolder();
        return server;
    }
}

