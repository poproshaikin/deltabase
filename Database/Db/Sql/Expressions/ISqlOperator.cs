using Sql.Tokens;

namespace Sql.Expressions;

public interface ISqlOperator
{
    SqlToken LeftOperand { get; }
    SqlToken Operator { get; }
    SqlToken RightOperand { get; }
}