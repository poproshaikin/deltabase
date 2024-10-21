using Sql.Expressions;

namespace Sql.Queries;

public class InsertQuery : SqlQuery
{
    public IntoExpr Into { get; set; }
    
    public ValuesExpr[] Values { get; init; }
}