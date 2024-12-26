namespace Sql.Shared.Expressions;

public class InsertExpr : SqlExpr
{
    public string TableName { get; set; }
    public string[] ColumnNames { get; set; }
}