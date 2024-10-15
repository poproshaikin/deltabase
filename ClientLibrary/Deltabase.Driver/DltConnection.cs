using System.Data.Common;

namespace Deltabase.Driver;

public class DltConnection : DbConnection
{
    // <serverName>;<dbName>;<serverHost>;<serverPort>;<password>

    private string _connectionString;
    private bool _isOpen;

    /// <inheritdoc/>
    public override string ConnectionString
    {
        get => _connectionString;
        set => _connectionString = value;
    }
    
    /// <inheritdoc/>
    public DltConnection(string connectionString) : base()
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc/>
    public override void Open()
    {
        
    }

    /// <inheritdoc/>
    public override void Close()
    {
        
    }

    protected override DltCommand CreateDbCommand()
    {
        return new DltCommand()
    }

    public DltCommand CreateDbCommand(string sql)
    {
        
    }
}