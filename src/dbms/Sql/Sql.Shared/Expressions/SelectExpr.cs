namespace Sql.Shared.Expressions;

public class SelectExpr : SqlExpr
{
    public bool AllColumns { get; set; }
    public string[]? ColumnNames { get; set; }
}