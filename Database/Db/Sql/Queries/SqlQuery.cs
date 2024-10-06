using Enums.Sql.Queries;
using Sql.Expressions;
using Sql.Tokens;

namespace Sql.Queries;

public abstract class SqlQuery
{
    public SqlExpression[] Expressions { get; private set; }
    public QueryType Type { get; private set; }
    
    protected SqlQuery(IReadOnlyList<SqlExpression> expressions, QueryType type)
    {
        Expressions = expressions.ToArray();
        Type = type;
    }
}