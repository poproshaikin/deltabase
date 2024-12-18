using Sql.Shared.Expressions;
using Sql.Shared.Queries.Interfaces;

namespace Sql.Shared.Queries;

public class DeleteQuery : ISqlQuery, IParsedQuery, IValidatedQuery
{
    public FromExpr From { get; set; }
    
    public ConditionGroup? Condition { get; set; }
    public LimitExpr? Limit { get; set; }
}