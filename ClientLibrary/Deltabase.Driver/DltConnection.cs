using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Enums.Tcp;
using Utils;

namespace Deltabase.Driver;

public class DltConnection : DbConnection
{
    // <serverName>;<dbName>;<serverHost>;<serverPort>;<password>
    // connection string from testing client example
    // firstServer;firstDb;127.0.0.1;5678;12345678

    private string _connectionString;
    private bool _isOpen;
    private ConnectionState _state;

    private TcpHelper _tcp;

    public override string ConnectionString
    {
        get => _connectionString;
        set => _connectionString = value;
    }

    public override string DataSource => GetConnectionStringElement(0);
    
    public override string Database => GetConnectionStringElement(1);
    
    public override ConnectionState State => _state;
    
    public override string ServerVersion => GetServerVersion();

    /// <inheritdoc/>
    public DltConnection(string connectionString) : base()
    {
        _connectionString = connectionString;
        HashPasswordInConnectionString();

        _state = ConnectionState.Closed;
        
        string address = GetConnectionStringElement(2);
        string port = GetConnectionStringElement(3);
        
        _tcp = new TcpHelper(address, int.Parse(port));
    }
    
#region Method overrides

    /// <inheritdoc/>
    public override void Open()
    {
        _tcp.Connect();

        try
        {
            string testResponse = TestConnection();

            if (testResponse == DataSource) // compares response with the server name. if equals, test was successfull
                TryOpenConnection();
            else
                throw new DltException(
                    $"Connection test failed. Expected response: '{DataSource}', but received: '{testResponse}'.");
        }
        catch (SocketException ex)
        {
            throw new DltException($"Failed to open connection.", ex);
        }
    }

    /// <inheritdoc/>
    public override void Close()
    {
        throw new NotImplementedException();
    }

    protected override DltCommand CreateDbCommand()
    {
        return new DltCommand(this);
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotImplementedException();
    }

    public override void ChangeDatabase(string databaseName)
    {
        throw new NotImplementedException();
    }
    
#endregion

#region Private logic

    private string GetConnectionStringElement(int index)
    {
        return _connectionString.Split(';')[index];
    }

    private string TestConnection()
    {
        const string command = "~test■ ";
        _tcp.Write(command);
        
        return _tcp.Read(); // if success, returns a name of a connected server
    }

    private void TryOpenConnection()
    {
        ResponseType response = TryConnect();

        switch (response)
        {
            case ResponseType.ConnectedSuccessfully:
            {
                _isOpen = true;
                _state = ConnectionState.Open;
                break;
            }
            default:
                ThrowConnectionException(response);
                break;
        }
    }

    private ResponseType TryConnect()
    {
        string command = $"~connect■ {_connectionString}";

        try
        {
            _tcp.Write(command);
            string responseRaw = _tcp.Read();

            return (ResponseType)int.Parse(responseRaw);
        }
        catch (SocketException ex)
        {
            throw new DltException("Failed to open connection.", ex);
        }
    }

    public string GetServerVersion()
    {
        throw new NotImplementedException();
    }

    private void HashPasswordInConnectionString()
    {
        string password = GetConnectionStringElement(4);
        string hashedPassword = ConvertHelper.Sha256(password);
        
        _connectionString = _connectionString.Replace(password, hashedPassword);
    }

    [DoesNotReturn]
    private void ThrowConnectionException(ResponseType response)
    {
        throw response switch
        {
            ResponseType.DatabaseNameIsNotSpecified => new DltException("Database name is not specified in the connection string"),
            ResponseType.InvalidPassword => new DltException("Invalid password passed in the connection string"),
            ResponseType.Unauthorized => new DltException("Connection authorization failed"),
            ResponseType.NoAvailableSlots => new DltException("A database's server has not available slots for the connection"),
            
            _ => throw new DltException(),
        };
    }
    
#endregion
}