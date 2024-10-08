using Sql.Expressions;
using Sql.Tokens;

namespace Sql.Common.Queries;

public class SelectQuery : SqlQuery, IQueryWithPassedColumns, IQueryWithTableName, IQueryWithCondition
{
    public SqlToken TableName { get; init; }
    public bool SelectAllColumns { get; init; }
    public SqlToken[] PassedColumns { get; set; }
    
    public ConditionGroup? Condition { get; init; }

    public SelectQuery(IReadOnlyList<SqlToken> tokens) : base(tokens)
    {
    }
}