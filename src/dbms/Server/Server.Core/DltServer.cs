using System.Net;
using Enums.Network;
using Network;
using Server.Core.Sessions;
using Sql.Shared;
using Utils;
using Utils.Settings;

namespace Server.Core;

public class DltServer
{
    private const int connected_clients_limit = 15;
    private const int session_lifetime_minutes = 30;
    private const string operation_result_key = "result";
    
    private string _serverName;
    
    private IConnectionsPool _connectionsPool;

    private readonly FileSystemHelper _fs;
    private readonly SemaphoreSlim _semaphore;   
    private readonly AuthService _auth;

    private ServerSettings _serverSettings;
    
    public DltServer(string serverName)
    {
        _serverName = serverName;
        _fs = new FileSystemHelper(serverName);

        _serverSettings = _fs.GetServerSettings();
            
        _semaphore = new SemaphoreSlim(connected_clients_limit);
        _auth = new AuthService(TimeSpan.FromMinutes(session_lifetime_minutes));

        _connectionsPool = IConnectionsPool.FromProtocol(_serverSettings.TransportProtocol);
        _connectionsPool.WithListener(_serverSettings.Address, _serverSettings.Port);
    }

    public void Start(CancellationToken token)
    {
        Logger.LogHeader($"Server started successfully on end point: {_connectionsPool.ListeningEndPoint}");
        
        while (!token.IsCancellationRequested)
        {
            IConnection connection = _connectionsPool.AwaitPendingConnection();
            ProcessRequestAsync(connection);
        }
    }

    public void Stop(bool closeConnections = false)
    {
        _connectionsPool.Stop(closeConnections);
    }

    private async void ProcessRequestAsync(IConnection connection)
    {
        IHandler handler = connection.GetHandler();
        
        string firstSet = handler.ReadToEnd();
        
        switch (firstSet[0..2])  // con / sql / cls
        {
            case "con":
                await ProcessConnectAsync(connection, firstSet[4..]);
                break;
            // case RequestType.sql:
            //     await ProcessSqlAsync(connection, requestSettings);
            //     break;
            // case RequestType.close:
            //     await ExecuteCloseAsync(connection, requestSettings);
            //     break;
        }
    }

    private async Task ProcessConnectAsync(IConnection connection, string serverName)
    {
        IHandler handler = connection.GetHandler();
        
        if (serverName != _serverName ||
            _semaphore.CurrentCount < connected_clients_limit)
        {
            await handler.WriteAsync(0);
            connection.Close();
            return;
        }

        await handler.WriteAsync(1);
        
        string secondSet = handler.ReadToEnd();
        string[] secondSetParts = secondSet.Split(';');

        if (secondSetParts.Length != 3)
        {
            await handler.WriteAsync($"{(byte)0};{(int)ResponseType.InvalidRequest}");
            connection.Close();
            return;
        }
        
        string dbName = secondSetParts[0];
        string password = secondSetParts[1];
        string clientToken = secondSetParts[2];

        ResponseType? error = null;

        if (_serverSettings.PasswordHashed != password)
        {
            error = ResponseType.InvalidPassword;
        }
        if (_fs.ExistsDatabase(dbName))
        {
            error = ResponseType.DatabaseDoesntExist;
        }

        if (error is not null)
        {
            await handler.WriteAsync($"{(byte)0};{(int)error}");
            connection.Close();
            return;
        }

        SessionInfo session = _auth.RegisterSession();
        session.AssociatedDatabase = new DltDatabase(dbName, _serverName);
        session.ClientTokenHash = clientToken;

        await handler.WriteAsync($"1;{session.ServerTokenHash}");
    }
    
    // private async Task ProcessConnectAsync(IConnection connection, RequestSettings requestSettings)
    // {
    //     ResponseSettings responseSettings = new ResponseSettings();
    //
    //     bool authorized = true;
    //     
    //     if (requestSettings.ServerName != _serverSettings.Name)
    //     {
    //         responseSettings.Add(operation_result_key, ResponseType.WrongServerAccessed);
    //         authorized = false;
    //     }
    //     
    //     if (requestSettings.Password != _serverSettings.PasswordHashed)
    //     {
    //         responseSettings.Add(operation_result_key, ResponseType.InvalidPassword);
    //         authorized = false;
    //     }
    //     
    //     if (_semaphore.CurrentCount < connected_clients_limit)
    //     {
    //         responseSettings.Add(operation_result_key, ResponseType.NoAvailableSlots);
    //         authorized = false;
    //     }
    //
    //     if (authorized)
    //     {
    //         responseSettings.Add(operation_result_key, ResponseType.AccessGranted);
    //         
    //         SessionInfo session = _auth.RegisterSession();
    //         
    //         session.AssociatedDatabase = new DltDatabase(requestSettings.DatabaseName, _serverName);
    //         session.ClientTokenHash = requestSettings.ClientToken;
    //         
    //         responseSettings.Add("server_token", session.ServerTokenHash);
    //     }
    //
    //     await RespondSettings(connection, responseSettings);
    // }
    //
    // private async Task ProcessSqlAsync(IConnection connection, RequestSettings requestSettings)
    // {
    //     if (_auth.TryAuthorizeSession(requestSettings.CombinedToken, out SessionInfo session))
    //     {
    //         DltDatabase database = session.AssociatedDatabase;
    //         
    //         string sqlRequest = requestSettings.SqlQuery;
    //         ExecutionResult result = database.ExecuteRequest(sqlRequest);
    //
    //         await StartBatchSession(connection, result);
    //     }
    //     else
    //     {
    //         await RespondUnauthorized(connection);
    //     }
    // }
    //
    // private async Task ExecuteCloseAsync(IConnection connection, RequestSettings requestSettings)
    // {
    //     bool closed = _auth.CloseSession(requestSettings.CombinedToken);
    //     ResponseType result = closed ? ResponseType.Success : ResponseType.Failure;
    //     
    //     ResponseSettings response = new();
    //     response.Add(operation_result_key, (int)result);
    //
    //     await RespondSettings(connection, response);
    //     _connectionsPool.Remove(connection);
    // }
    //
    // private async Task RespondUnauthorized(IConnection connection)
    // {
    //     ResponseSettings response = new ResponseSettings();
    //     response.Add(operation_result_key, ResponseType.Unauthorized);
    //
    //     await RespondSettings(connection, response);
    // }
    //
    // private async Task RespondSettings(IConnection connection, SettingsCollection settings)
    // {
    //     IHandler handler = connection.GetHandler();
    //     await handler.WriteAsync(settings);
    // }
    //
    // private async Task StartBatchSession(IConnection connection, ExecutionResult result)
    // {
    //     
    // }
    
    public static DltServer Create(string serverName, ushort parse, string password)
    {
        throw new NotImplementedException();
    }
    
    
    
    // private async Task ProcessConnectAsync(IRequest request, RequestSettings requestSettings)
    // {
    //     IHandler handler = request.GetHandler();
    //     
    //     ResponseSettings responseSettings = new ResponseSettings();
    //
    //     bool authorized = true;
    //     
    //     if (requestSettings.ServerName != _serverSettings.Name)
    //     {
    //         responseSettings.Add(operation_result_key, ResponseType.WrongServerAccessed);
    //         authorized = false;
    //     }
    //     
    //     if (requestSettings.Password != _serverSettings.PasswordHashed)
    //     {
    //         responseSettings.Add(operation_result_key, ResponseType.InvalidPassword);
    //         authorized = false;
    //     }
    //     
    //     if (_semaphore.CurrentCount < connected_clients_limit)
    //     {
    //         responseSettings.Add(operation_result_key, ResponseType.NoAvailableSlots);
    //         authorized = false;
    //     }
    //
    //     if (authorized)
    //     {
    //         responseSettings.Add(operation_result_key, ResponseType.AccessGranted);
    //         
    //         SessionInfo session = _auth.RegisterSession();
    //         
    //         session.AssociatedDatabase = new DltDatabase(requestSettings.DatabaseName, _serverName);
    //         session.ClientTokenHash = requestSettings.ClientToken;
    //         
    //         responseSettings.Add("server_token", session.ServerTokenHash);
    //     }
    //
    //     await RespondSettings(request, responseSettings);
    // }
    //
    // private async Task ProcessSqlAsync(IRequest request, RequestSettings requestSettings)
    // {
    //     if (_auth.TryAuthorizeSession(requestSettings.CombinedToken, out SessionInfo session))
    //     {
    //         DltDatabase database = session.AssociatedDatabase;
    //         
    //         string sqlRequest = requestSettings.SqlQuery;
    //         ResponseSettings sqlResponse = database.ExecuteRequest(sqlRequest)
    //             .ToResponseSettings();
    //
    //         await RespondSettings(request, sqlResponse);
    //     }
    //     else
    //     {
    //         await RespondUnauthorized(request);
    //     }
    // }
    //
    // private async Task ExecuteCloseAsync(IRequest request, RequestSettings requestSettings)
    // {
    //     bool closed = _auth.CloseSession(requestSettings.CombinedToken);
    //     ResponseType result = closed ? ResponseType.Success : ResponseType.Failure;
    //     
    //     ResponseSettings response = new();
    //     response.Add(operation_result_key, (int)result);
    //
    //     await RespondSettings(request, response);
    //     _connectionsPool.RemoveByRequest(request);
    // }
    //
    // private async Task RespondUnauthorized(IRequest request)
    // {
    //     ResponseSettings response = new ResponseSettings();
    //     response.Add(operation_result_key, ResponseType.Unauthorized);
    //
    //     await RespondSettings(request, response);
    // }
    //
    // private async Task RespondSettings(IRequest acceptedRequest, SettingsCollection settings)
    // {
    //     IHandler handler = acceptedRequest.GetHandler();
    //     await handler.WriteAsync(settings);
    // }
    //
    // public static DltServer Create(string serverName, ushort parse, string password)
    // {
    //     throw new NotImplementedException();
    // }
}