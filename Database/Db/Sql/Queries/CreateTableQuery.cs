using Sql.Expressions;
using Sql.Tokens;

namespace Sql.Queries;

public class CreateTableQuery : SqlQuery, IQueryWithTableName
{
    public SqlToken TableName { get; set; }
    public ColumnDefExpression[] NewColumns { get; set; }
       
    public CreateTableQuery(IReadOnlyList<SqlToken> tokens) : base(tokens)
    {
    }
}