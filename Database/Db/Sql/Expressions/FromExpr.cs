namespace Sql.Expressions;

public class FromExpr : SqlExpression
{
    public string TableName { get; init; }
}