namespace Sql.Shared.Expressions;

public class AssignGroup : SqlExpr
{
    public AssignExpr[] Assignments { get; set; }

    public AssignGroup(AssignExpr[] assignments)
    {
        Assignments = assignments;
    }
}