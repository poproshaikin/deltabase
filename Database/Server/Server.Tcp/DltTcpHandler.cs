using System.Net.Sockets;
using Enums.Tcp;
using Server.Core;
using Utils;

namespace Server.Tcp;

/// <summary>
/// An abstraction over the <see cref="TcpClient"/> class for convenient connection establishing, reading and writing operations
/// </summary>
public class DltTcpHandler : DltTcpService, IDisposable
{
    private TcpClient _currentClient; 
    private CancellationTokenSource _cancellationTokenSource;

    private List<TcpRequest> _requests;
    public IReadOnlyList<TcpRequest> Requests => _requests;

    /// <summary>
    /// Public property returning a protected field of the base class
    /// </summary>
    public new DltConnectionConfig? ConnectionConfig
    {
        get => base.ConnectionConfig;
        set => base.ConnectionConfig = value;
    }
    
    /// <summary>
    /// Gets a value indicating whether the object has been disposed via a call to the <see cref="Dispose"/> method.
    /// </summary>
    /// <value>
    /// <c>true</c> if the object has been disposed and should no longer be used; otherwise, <c>false</c>.
    /// </value>
    public bool IsDisposed { get; private set; }

    public DltTcpHandler(TcpClient client, DltConnectionConfig? cnnConfig = null) : base(cnnConfig)
    {
        _currentClient = client;
        _cancellationTokenSource = new CancellationTokenSource();
        _requests = new List<TcpRequest>();
        IsDisposed = false;
        
        StartReceivingAsync();
    }
    
    /// <summary>
    /// Starts asynchronously receiving requests from the <see cref="_currentClient"/>'s network stream
    /// and stores them
    /// </summary>
    private async void StartReceivingAsync()
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        while (!cancellationToken.IsCancellationRequested)
        {
            string message = await ReadAsync();
            _requests.Add(new TcpRequest(message));
        }
    }
    
    public async Task<TcpRequest> AwaitRequestAsync()
    {
        await WaitForRequest();

        TcpRequest first = _requests[0];
        _requests.RemoveAt(0);
        return first;
    }

    public async Task WaitForRequest() => await WaitForRequest(_cancellationTokenSource.Token);
    
    private async Task WaitForRequest(CancellationToken cancellationToken)
    {
        if (_requests.Count > 0)
        {
            return;
        }
        
        while (_requests.Count == 0) 
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await Task.Delay(100, cancellationToken);
        }
    }
    
    public void Write(string message) => base.Write(_currentClient, message);
    
    public void Write(byte[] message) => Write(ParseHelper.GetString(message));
    
    public void Write(TcpResponseType response) => Write(((int)response).ToString());
    
    public string Read() => base.Read(_currentClient);
    
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task<string> ReadAsync() => await ReadAsync(CancellationToken.None);

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task<string> ReadAsync(CancellationToken cancellationToken) =>
        await base.ReadAsync(_currentClient, cancellationToken);

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _requests = null!;
        IsDisposed = true;
    }
}