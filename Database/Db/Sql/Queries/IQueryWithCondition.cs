using Sql.Expressions;

namespace Sql.Queries;

public interface IQueryWithCondition
{
    ConditionGroup? Condition { get; }
}