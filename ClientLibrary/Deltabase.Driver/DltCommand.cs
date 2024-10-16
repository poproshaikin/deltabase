using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Deltabase.Driver;

public class DltCommand : DbCommand
{
    private DltConnection _connection;
    private string _commandText;
    private int _timeout;

    public DltCommand()
    {
    }

    public DltCommand(DltConnection connection)
    {
        _connection = connection;
    }

    public DltCommand(string commandText, DltConnection connection) : this(connection)
    {
        _commandText = commandText;
    }

    public DltCommand(string commandText, DltConnection connection, int timeout) : this(commandText, connection)
    {
        _timeout = timeout;
    }
    
    protected override DbConnection? DbConnection
    {
        get => _connection;
        set => _connection = (DltConnection)value!;
    }

    [AllowNull]
    public override string CommandText
    {
        get => _commandText;
        set => _commandText = value ?? "";
    }

    public override int CommandTimeout
    {
        get => _timeout;
        set => _timeout = value;
    }

    public override CommandType CommandType { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; }
    protected override DbTransaction? DbTransaction { get; set; }
    public override bool DesignTimeVisible { get; set; }
    
    public override void Cancel()
    {
        throw new NotImplementedException();
    }

    public override int ExecuteNonQuery()
    {
        throw new NotImplementedException();
    }

    public override object? ExecuteScalar()
    {
        throw new NotImplementedException();
    }

    public override void Prepare()
    {
        throw new NotImplementedException();
    }

    protected override System.Data.Common.DbParameter CreateDbParameter()
    {
        throw new NotImplementedException();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        throw new NotImplementedException();
    }
}