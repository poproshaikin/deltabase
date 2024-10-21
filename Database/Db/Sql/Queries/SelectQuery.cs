using Sql.Expressions;

namespace Sql.Queries;

public class SelectQuery : SqlQuery, IValidatedQuery, IParsedQuery
{
    public SelectExpr Select { get; private set; }
    public FromExpr From { get; private set; }
    
    public ConditionGroup? Condition { get; init; }         
    public LimitExpr? Limit { get; init; }  
}