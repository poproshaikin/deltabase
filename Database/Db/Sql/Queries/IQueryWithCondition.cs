using Sql.Expressions;

namespace Sql.Common.Queries;

public interface IQueryWithCondition
{
    ConditionGroup? Condition { get; }
}