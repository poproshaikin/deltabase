namespace Network;

public interface IConnection : IDisposable
{
    Guid Id { get; }
    DateTime LastActivity { get; }
    bool IsConnected { get; }

    void Close();
    IHandler GetHandler();
}