using Sql.Tokens;

// ReSharper disable once CheckNamespace
namespace Sql.Interfaces;

public interface IWithPassedColumns
{
    SqlToken[] PassedColumns { get; }
}

public interface IWithPassedValues
{
    SqlToken[] PassedValues { get; }
}

public interface IWithTableName
{
    public SqlToken TableName { get; }
}