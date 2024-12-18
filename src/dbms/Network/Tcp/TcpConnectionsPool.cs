using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Utils;

namespace Network.Tcp;

internal class TcpConnectionsPool : IConnectionsPool
{
    private readonly ConcurrentDictionary<Guid, TcpConnection> _pool;
    private readonly ConcurrentQueue<TcpConnection> _requestsQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    private readonly Task _pendingDataListeningTask;
    private readonly Task _timeoutMonitoringTask;

    private TcpListener? _listener;
    private Task _newConnectionsMonitoringTask;

    private const int listening_interval_milliseconds = 150;
    private const int connection_timeout_monitoring_minutes = 1;
    private const int connection_timeout_minutes = 1;

    public EndPoint? ListeningEndPoint => _listener?.LocalEndpoint;

    internal TcpConnectionsPool()
    {
        _pool = new ConcurrentDictionary<Guid, TcpConnection>();
        _requestsQueue = new ConcurrentQueue<TcpConnection>();
        _cancellationTokenSource = new CancellationTokenSource();
        
        _pendingDataListeningTask = MonitorNewRequests();
        _timeoutMonitoringTask = MonitorConnectionsTimeout();
    }

    public IConnection AwaitPendingConnection()
    {
        while (_requestsQueue.IsEmpty)
        {
            Thread.Sleep(listening_interval_milliseconds);
        }
        
#pragma warning disable CS8600 CS8603 // This method waits until queue won't be empty, so TryDequeue always returns connection 
        _ = _requestsQueue.TryDequeue(out TcpConnection connection);
        return connection;
#pragma warning restore CS8600 CS8603
    }

    public void WithListener(string address, int port)
    {
        _listener = new TcpListener(localaddr: IPAddress.Parse(address), port);
        _listener.Start();
        _newConnectionsMonitoringTask = MonitorNewConnections();
    }
    
    public void Add(IConnection connection)
    {
        TcpConnection tcpConnection = (TcpConnection)connection;
        
        _ = _pool.TryAdd(tcpConnection.Id, tcpConnection);
    }

    public void RemoveByRequest(IRequest request)
    {
        TcpRequest tcpRequest = (TcpRequest)request;
        TcpClient client = tcpRequest.Sender;

        TcpConnection? connection = FindConnectionInPool(client);
        
        if (connection is not null) 
            _pool.TryRemove(connection.Id, out _);
    }   

    public void Remove(IConnection request)
    {
        TcpConnection tcpConnection = (TcpConnection)request;
        
        _pool.Remove(tcpConnection.Id, out _);
    }

    public void Dispose() => Stop(closeConnections: false);

    public void Stop(bool closeConnections)
    {
        _cancellationTokenSource.Cancel();

        _newConnectionsMonitoringTask.Dispose();
        _timeoutMonitoringTask.Dispose();
        _pendingDataListeningTask.Dispose();
        
        if (closeConnections) 
            CloseConnections();
    }

    private async Task MonitorNewRequests()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            await CheckCollectionForAvailableData();
            
            await Task.Delay(listening_interval_milliseconds);
        }
    }

    private async Task MonitorConnectionsTimeout()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            foreach (TcpConnection connection in _pool.Values)
            {
                if (connection.IsConnected is false)
                {
                    _ = _pool.Remove(connection.Id, out _);
                }
                
                if (DateTime.Now - connection.LastActivity >= TimeSpan.FromMinutes(connection_timeout_minutes))
                {
                    connection.Close();
                    _ = _pool.Remove(connection.Id, out _);
                }
            }
            
            await Task.Delay(TimeSpan.FromMinutes(connection_timeout_monitoring_minutes));
        }
    }

    private async Task MonitorNewConnections()
    {
        if (_listener is null)
            throw new NullReferenceException("The listener was not initialized before monitoring new connections in the pool.");
        
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();
            TcpConnection connection = new(client);
            Add(connection);

            await Task.Delay(listening_interval_milliseconds);
        }
    }

    private Task CheckCollectionForAvailableData()
    {
        foreach (TcpConnection connection in _pool.Select(kvp => kvp.Value))
        {
            if (connection.Socket.Available > 0)
            {
                _requestsQueue.Enqueue(connection);
            }
        }

        return Task.CompletedTask;
    }

    private async Task<TcpRequest> ReadAndParseRequestAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[client.Available];
        
        _ = await stream.ReadAsync(buffer);
        
        string content = ConvertHelper.GetString(buffer);
        return new TcpRequest(sender: client, content);
    }

    private void CloseConnections()
    {
        foreach (TcpConnection connection in _pool.Values)
        {
            connection.Close();
        }
    }

    private TcpConnection? FindConnectionInPool(TcpClient client)
    {
        return _pool.FirstOrDefault(kvp => kvp.Value.Client == client).Value;
    } 
}