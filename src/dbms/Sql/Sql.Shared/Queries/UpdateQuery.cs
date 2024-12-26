using Sql.Shared.Expressions;
using Sql.Shared.Queries.Interfaces;

namespace Sql.Shared.Queries;

public class UpdateQuery : ISqlQuery, IParsedQuery, IValidatedQuery
{
    public UpdateExpr Update { get; set; }
    public SetExpr Set { get; set; }
    public ConditionGroup? Condition { get; set; }
}