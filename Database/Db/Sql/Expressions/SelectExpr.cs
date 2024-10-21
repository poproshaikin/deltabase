using Sql.Tokens;

namespace Sql.Expressions;

public class SelectExpr : SqlExpression
{
    public bool AllColumns { get; init; }
    public string[]? ColumnNames { get;  init; }
}