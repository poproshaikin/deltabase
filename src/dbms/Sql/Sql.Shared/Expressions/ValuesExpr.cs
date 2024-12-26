namespace Sql.Shared.Expressions;

public class ValuesExpr : SqlExpr
{
    public string[] Values { get; set; }

    public string this[int index] => Values[index];
}