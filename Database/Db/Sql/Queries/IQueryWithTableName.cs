using Sql.Tokens;

namespace Sql.Common.Queries;

public interface IQueryWithTableName
{
    public SqlToken TableName { get; init; }
}