using Sql.Expressions;
using Sql.Tokens;

namespace Sql.Common.Queries;

public class UpdateQuery : SqlQuery, IQueryWithTableName, IQueryWithCondition
{
    public SqlToken TableName { get; init; }
    public AssignExpr[] Assignments { get; init; }
    public ConditionGroup? Condition { get; init; }
    
    public UpdateQuery(IReadOnlyList<SqlToken> tokens) : base(tokens)
    {
    }
}