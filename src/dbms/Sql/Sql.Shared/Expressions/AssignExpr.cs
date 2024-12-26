using Enums.Sql.Tokens;
using Sql.Shared.Tokens;

namespace Sql.Shared.Expressions;

public class AssignExpr : SqlExpr, ISqlOperator
{
    public SqlToken LeftOperand { get; init; }
    public SqlToken Operator { get; }

    public SqlToken RightOperand { get; init; }
    
    public AssignExpr()
    {
        Operator = new SqlToken(TokenType.Operator, "=");
    }
}