using Sql.Tokens;

namespace Sql.Queries;

public class InsertQuery : SqlQuery, IQueryWithPassedColumns, IQueryWithPassedValues, IQueryWithTableName
{
    public SqlToken TableName { get; init; }
    public SqlToken[] PassedColumns { get; set; }
    public SqlToken[] PassedValues { get; set; }

    public InsertQuery(IReadOnlyList<SqlToken> tokens) : base(tokens)
    {
    }
}