using Sql.Shared.Tokens;

namespace Sql.Shared.Expressions;

public class ConditionGroup : SqlExpr
{
    public ConditionExpr[] Subconditions { get; private set; }
    public SqlToken[] LogicalOperators { get; private set; }
    
    public ConditionGroup(IReadOnlyList<ConditionExpr> subconditions, IReadOnlyList<SqlToken> logicalOperators)
    {
        Subconditions = subconditions is ConditionExpr[] arr1 ? arr1 : subconditions.ToArray();
        LogicalOperators = logicalOperators is SqlToken[] arr2 ? arr2 : logicalOperators.ToArray();
    }
    
    public SqlToken[] GetLeftOperands() => Subconditions.Select(expr => expr.LeftOperand).ToArray();
    public SqlToken[] GetRightOperands() => Subconditions.Select(expr => expr.RightOperand).ToArray();
}