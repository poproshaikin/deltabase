using Enums.Exceptions;
using Exceptions;
using Sql.Shared.Tokens;

namespace Sql.Shared.Expressions;

public class ConditionExpr : SqlExpr, ISqlOperator
{
    public SqlToken LeftOperand { get; init; }
    public SqlToken Operator { get; init; }
    public SqlToken RightOperand { get; init; }
    
    public ConditionExpr(IReadOnlyList<SqlToken> tokens)
    {
        LeftOperand = tokens[0];
        Operator = tokens[1].IsOperator() ? tokens[1] : throw new DbEngineException(ErrorType.InvalidCondition);
        RightOperand = tokens[2];
    }
}