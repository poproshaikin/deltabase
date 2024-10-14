using System.Net.Sockets;
using Enums.Tcp;
using Utils;

namespace Server.Tcp;

/// <summary>
/// An abstraction over the <see cref="TcpClient"/> class for convenient connection establishing, reading and writing operations
/// </summary>
public class DltTcpHandler : DltTcpService, IDisposable
{
    private TcpClient _currentClient; 
    private CancellationTokenSource _cancellationTokenSource;
    private Queue<TcpRequest> _requests;
    
    /// <summary>
    /// Gets the list of TCP requests received by this handler.
    /// </summary>
    public IReadOnlyCollection<TcpRequest> Requests => _requests;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="DltTcpHandler"/> class with the specified <see cref="TcpClient"/>.
    /// Optionally, a <see cref="DltConnectionConfig"/> can be passed to configure the handler.
    /// </summary>
    /// <param name="client">The <see cref="TcpClient"/> used for the connection.</param>
    /// <param name="cnnConfig">The optional connection configuration.</param>
    public DltTcpHandler(TcpClient client, DltConnectionConfig? cnnConfig = null) : base(cnnConfig)
    {
        _currentClient = client;
        _cancellationTokenSource = new CancellationTokenSource();
        _requests = [];
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
            string message = await ReadAsync(cancellationToken);
            _requests.Enqueue(new TcpRequest(message));
        }
    }
    
    /// <summary>
    /// Asynchronously waits for and retrieves the next request from the list of requests.
    /// </summary>
    /// <returns>A <see cref="TcpRequest"/> containing the next client request.</returns>
    public async Task<TcpRequest> AwaitRequestAsync()
    {
        await WaitForRequest();

        if (_cancellationTokenSource.IsCancellationRequested)
            throw new OperationCanceledException();
        
        return _requests.Dequeue();
    }
    
    /// <summary>
    /// Waits asynchronously for a new request to be received, considering the cancellation token.
    /// </summary>
    private async Task WaitForRequest()
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;
        
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
    
    /// <summary>
    /// Writes a message to the client's network stream.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public void Write(string message) => base.Write(_currentClient, message);
    
    /// <summary>
    /// Writes a byte array as a message to the client's network stream.
    /// </summary>
    /// <param name="message">The message in bytes to write.</param>
    public void Write(byte[] message) => Write(ParseHelper.GetString(message));
    
    /// <summary>
    /// Writes a predefined TCP response to the client's network stream.
    /// </summary>
    /// <param name="response">The <see cref="ResponseType"/> to send as a response.</param>
    public void Write(ResponseType response) => Write(((int)response).ToString());
    
    /// <summary>
    /// Reads a message from the client's network stream.
    /// </summary>
    /// <returns>The message received as a string.</returns>
    public string Read() => base.Read(_currentClient);
    
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Asynchronously reads a message from the client's network stream.
    /// </summary>
    /// <returns>A task that represents the asynchronous read operation, with the message as a result.</returns>
    public async Task<string> ReadAsync() => await ReadAsync(CancellationToken.None);
    
    /// <summary>
    /// Asynchronously reads a message from the client's network stream, considering the provided cancellation token.
    /// </summary>
    /// <param name="cancellationToken">The token to observe for cancellation.</param>
    /// <returns>A task that represents the asynchronous read operation, with the message as a result.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task<string> ReadAsync(CancellationToken cancellationToken) =>
        await base.ReadAsync(_currentClient, cancellationToken);

    /// <summary>
    /// Releases the resources used by the <see cref="DltTcpHandler"/> instance.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _requests = null!;
        IsDisposed = true;
    }
}