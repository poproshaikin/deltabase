using Sql.Interfaces;
using Sql.Tokens;

namespace Sql.Expressions;

public class SelectExpression : SqlExpression, IWithPassedColumns
{
    public SqlToken[] PassedColumns { get; set; }

    public SelectExpression(params SqlToken[] passedColumns)
    {
        PassedColumns = passedColumns;
    }
}

public class FromExpression : SqlExpression, IWithTableName
{
    public SqlToken TableName { get; set; }

    public FromExpression(SqlToken tableName)
    {
        TableName = tableName;
    }
}