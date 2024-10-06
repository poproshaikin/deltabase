using Sql.Tokens;

namespace Sql.Expressions;

public abstract class SqlExpression
{
    public SqlToken[] Tokens { get; private set; }
}