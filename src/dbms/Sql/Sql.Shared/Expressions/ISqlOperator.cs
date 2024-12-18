using Sql.Shared.Tokens;

namespace Sql.Shared.Expressions;

public interface ISqlOperator
{
    SqlToken LeftOperand { get; }
    SqlToken Operator { get; }
    SqlToken RightOperand { get; }
}