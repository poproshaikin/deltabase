using System.Net.Sockets;
using Utils;

namespace Server.Tcp;

/// <summary>
/// An abstract class providing TCP communication functionalities, including reading and writing data to a <see cref="TcpClient"/>'s <see cref="NetworkStream"/>.
/// </summary>
public abstract class DltTcpService
{
    /// <summary>
    /// Stores the connection configuration used by the service.
    /// </summary>
    protected DltConnectionConfig? ConnectionConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="DltTcpService"/> class with the provided connection configuration.
    /// </summary>
    /// <param name="cnnConfig">The connection configuration used by the service.</param>
    protected DltTcpService(DltConnectionConfig? cnnConfig)
    {
        ConnectionConfig = cnnConfig;
    }

    /// <summary>
    /// Writes the provided byte array buffer to the client's <see cref="NetworkStream"/>.
    /// </summary>
    /// <param name="client">The <see cref="TcpClient"/> whose <see cref="NetworkStream"/> will be used to send the data.</param>
    /// <param name="buffer">The byte array containing the message to write to the stream.</param>
    /// <exception cref="ArgumentNullException">Thrown if the client is null.</exception>
    protected void Write(TcpClient client, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        
        NetworkStream stream = client.GetStream();
        stream.Write(buffer);
        stream.Flush();
    }
    
    /// <summary>
    /// Writes the provided string message to the client's <see cref="NetworkStream"/> after converting it to bytes.
    /// </summary>
    /// <param name="client">The <see cref="TcpClient"/> whose <see cref="NetworkStream"/> will be used to send the message.</param>
    /// <param name="message">The string message to write to the stream.</param>
    /// <exception cref="ArgumentNullException">Thrown if the client is null.</exception>
    protected void Write(TcpClient client, string message) =>
        Write(client, ConvertHelper.GetBytes(message));

    /// <summary>
    /// Reads data from the <see cref="NetworkStream"/> of the connected <see cref="TcpClient"/> and returns it as a string.
    /// This method blocks the calling thread until the read operation completes.
    /// </summary>
    /// <param name="client">The connected <see cref="TcpClient"/>.</param>
    /// <returns>The data read from the client's <see cref="NetworkStream"/> as a string.</returns>
    protected string Read(TcpClient client)
    {
        Task<string> reading = ReadAsync(client, CancellationToken.None); 
        reading.Wait();
        return reading.Result;
    }

    /// <summary>
    /// Asynchronously reads data from the <see cref="NetworkStream"/> of the connected <see cref="TcpClient"/> and returns it as a string.
    /// </summary>
    /// <param name="client">The connected <see cref="TcpClient"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous read operation, containing the string read from the network stream.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the client is null.</exception>
    protected async Task<string> ReadAsync(TcpClient client, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        
        NetworkStream stream = client.GetStream();
        await WaitForDataAvailable(stream, cancellationToken);
        
        byte[] buffer = new byte[client.Available];
        _ = await stream.ReadAsync(buffer, cancellationToken);
        
        return ConvertHelper.GetString(buffer);
    }

    /// <summary>
    /// Waits until data is available to read from the provided <see cref="NetworkStream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="NetworkStream"/> to monitor.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous wait operation.</returns>
    protected static async Task WaitForDataAvailable(NetworkStream stream, CancellationToken cancellationToken)
    {
        while (!stream.DataAvailable && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken);
        }
    }
}