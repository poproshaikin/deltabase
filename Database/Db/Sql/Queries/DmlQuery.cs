using Enums.Sql.Queries;
using Sql.Expressions;

namespace Sql.Queries;

public class DmlQuery : SqlQuery
{
    
    
    public DmlQuery(IReadOnlyList<SqlExpression> expressions, QueryType type) : base(expressions, type)
    {
    }
}