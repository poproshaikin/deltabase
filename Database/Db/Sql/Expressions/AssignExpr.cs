using Enums.Sql.Tokens;
using Sql.Tokens;

namespace Sql.Expressions;

public class AssignExpr : SqlExpression, ISqlOperator
{
    private SqlToken _operator;
    
    public SqlToken LeftOperand { get; init; }
    public SqlToken Operator => _operator;
    public SqlToken RightOperand { get; init; }
    
    public AssignExpr(IEnumerable<SqlToken> tokens) : base(tokens)
    {
        _operator = new SqlToken(TokenType.Operator, "=");
    }
}