using Sql.Tokens;

namespace Sql.Common.Queries;

public class CreateTableQuery : SqlQuery, IQueryWithTableName
{
    public SqlToken TableName { get; init; }
       
    public CreateTableQuery(IReadOnlyList<SqlToken> tokens) : base(tokens)
    {
    }
}