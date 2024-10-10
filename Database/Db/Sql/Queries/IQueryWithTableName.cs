using Sql.Tokens;

namespace Sql.Queries;

public interface IQueryWithTableName
{
    public SqlToken TableName { get; }
}