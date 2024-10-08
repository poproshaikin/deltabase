using Sql.Tokens;
using Utils;

namespace Sql.Expressions;

public class ConditionExpr : SqlExpression, ISqlOperator
{
    public SqlToken LeftOperand { get; init; }
    public SqlToken Operator { get; init; }
    public SqlToken RightOperand { get; init; }
    
    public ConditionExpr(IReadOnlyList<SqlToken> tokens) : base(tokens)
    {
        LeftOperand = tokens[0];
        Operator = tokens[1].IsOperator() ? tokens[1] : throw ThrowHelper.InvalidToken(tokens[1]);
        RightOperand = tokens[2];
    }
}