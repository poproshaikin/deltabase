namespace Sql.Shared.Expressions;

public class ValuesExpr : SqlExpr
{
    public string[] Values { get; set; }

    public string this[int index] => Values[index];
    
    public static implicit operator string[](ValuesExpr valuesExpr) => valuesExpr.Values;
}