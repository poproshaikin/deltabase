using Sql.Expressions;
using Sql.Tokens;

namespace Sql.Common.Queries;

public class DeleteQuery : SqlQuery, IQueryWithPassedColumns, IQueryWithTableName, IQueryWithCondition
{
    public SqlToken TableName { get; init; }
    public SqlToken[] PassedColumns { get; set; }
    public ConditionGroup? Condition { get; set; }
    public bool SelectAllColumns { get; set; }
    
    public DeleteQuery(IReadOnlyList<SqlToken> tokens) : base(tokens)
    {
    }
}