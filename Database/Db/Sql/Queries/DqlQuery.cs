using Enums.Sql.Queries;
using Sql.Expressions;
using Sql.Interfaces;
using Sql.Tokens;

namespace Sql.Queries;

public class DqlQuery : SqlQuery, IWithTableName, IWithPassedColumns
{
    public SqlToken TableName => throw new NotImplementedException();
    public SqlToken[] PassedColumns => throw new NotImplementedException();
    public bool SelectAllColumns => throw new NotImplementedException();
    
    public DqlQuery(IReadOnlyList<SqlExpression> expressions, QueryType type) : base(expressions, type)
    {
    }
}