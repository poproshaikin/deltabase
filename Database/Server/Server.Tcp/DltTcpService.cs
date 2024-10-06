using System.Net.Sockets;
using Server.Core;
using Utils;

namespace Server.Tcp;

public abstract class DltTcpService
{
    protected DltConnectionConfig? ConnectionConfig;

    protected DltTcpService(DltConnectionConfig? cnnConfig)
    {
        ConnectionConfig = cnnConfig;
    }

    /// <summary>
    /// Writes the passed buffer to the client's network stream
    /// </summary>
    /// <param name="client"><see cref="TcpClient"/> to the <see cref="NetworkStream"/> of which data will be written</param>
    /// <param name="message">Message that will be written to the <see cref="NetworkStream"/></param>
    protected virtual void Write(TcpClient client, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        
        NetworkStream stream = client.GetStream();
        stream.Write(buffer);
        stream.Flush();
    }
    
    /// <summary>
    /// Writes the passed string parameter to the client's network stream
    /// </summary>
    /// <param name="client"><see cref="TcpClient"/> to the <see cref="NetworkStream"/> of which data will be written</param>
    /// <param name="message">Message that will be written to the <see cref="NetworkStream"/></param>
    protected virtual void Write(TcpClient client, string message) =>
        Write(client, ParseHelper.GetBytes(message));

    /// <summary>
    /// Reads data from the <see cref="NetworkStream"/> of the connected <see cref="TcpClient"/> and returns they as string
    /// </summary>
    /// <param name="client">a connected Tcp client</param>
    protected virtual string Read(TcpClient client)
    {
        Task<string> reading = ReadAsync(client, CancellationToken.None); 
        reading.Wait();
        return reading.Result;
    }

    /// <summary>
    /// Asynchronously reads data from the <see cref="networkStream"/> of the connected <see cref="TcpClient"/> and returns they as string
    /// </summary>
    /// <param name="client">a connected <see cref="TcpClient"/></param>
    protected virtual async Task<string> ReadAsync(TcpClient client, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        
        NetworkStream stream = client.GetStream();
        await WaitForDataAvailable(stream, cancellationToken);
        
        byte[] buffer = new byte[client.Available];
        _ = await stream.ReadAsync(buffer, cancellationToken);
        
        return ParseHelper.GetString(buffer);
    }

    protected static async Task WaitForDataAvailable(NetworkStream stream, CancellationToken cancellationToken)
    {
        while (!stream.DataAvailable && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken);
        }
    }
}