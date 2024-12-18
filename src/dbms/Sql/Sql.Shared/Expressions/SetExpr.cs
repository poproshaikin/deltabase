namespace Sql.Shared.Expressions;

public class SetExpr : SqlExpr
{
    public AssignExpr[] Assignments { get; set; }

    public SetExpr(AssignExpr[] assignments)
    {
        Assignments = assignments;
    }
}