using Sql.Shared.Expressions;
using Sql.Shared.Queries.Interfaces;

namespace Sql.Shared.Queries;

public class SelectQuery : ISqlQuery, IValidatedQuery, IParsedQuery
{
    public SelectExpr Select { get; set; }
    public FromExpr From { get; set; }
    
    public ConditionGroup? Condition { get; set; }         
    public LimitExpr? Limit { get; set; }  
}