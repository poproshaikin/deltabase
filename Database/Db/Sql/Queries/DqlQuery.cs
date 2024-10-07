using Enums.Sql.Queries;
using Enums.Sql.Tokens;
using Sql.Expressions;
using Sql.Interfaces;
using Sql.Tokens;
using Utils;

namespace Sql.Queries;

public class DqlQuery : SqlQuery, IWithTableName, IWithPassedColumns
{
    public SqlToken TableName => From.TableName;
    public SqlToken[] PassedColumns => Select.PassedColumns;
    public bool SelectAllColumns => Select!.SelectAllColumns;

    public SelectExpression Select =>
        (SelectExpression)Expressions.FirstOrDefault(e => e.GetType() == typeof(SelectExpression))!;
    
    public FromExpression From => 
        (FromExpression)Expressions.FirstOrDefault(e => e.GetType() == typeof(FromExpression))!;
    
    public DqlQuery(IReadOnlyList<SqlExpression> expressions, QueryType type) : base(expressions, type)
    {
    }
}