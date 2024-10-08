using Enums.Sql.Tokens;
using Sql.Interfaces;
using Sql.Tokens;

namespace Sql.Expressions;

public class SelectExpression : SqlExpression, IWithPassedColumns
{
    public SqlToken[] PassedColumns { get; set; }
    public bool SelectAllColumns { get; set; }

    public SelectExpression(params SqlToken[] passedColumns)
    {
        PassedColumns = passedColumns;
        SelectAllColumns = passedColumns.Length == 1 && passedColumns[0].IsOperator(OperatorType.Asterisk);
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

public class ConditionExpression : SqlExpression
{
    public SqlToken LeftOperand { get; set; }
    public SqlToken RightOperand { get; set; }
    public SqlToken Operator { get; set; }

    public ConditionExpression(
        SqlToken leftOperand,
        SqlToken rightOperand,
        SqlToken @operator)
    {
        LeftOperand = leftOperand;
        RightOperand = rightOperand;
        Operator = @operator;
    }
}