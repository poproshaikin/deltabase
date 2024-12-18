using Sql.Shared.Tokens;

namespace Sql.Shared.Expressions;

public class ColumnDefExpression
{
    public SqlToken ColumnName { get; set; }
    public SqlToken ValueType { get; set; }
    public SqlToken[] Constraints { get; set; }

    public ColumnDefExpression(SqlToken columnName, SqlToken valueType, IReadOnlyList<SqlToken>? constraints = null)
    {
        ColumnName = columnName;
        ValueType = valueType;
        Constraints = constraints is null ? [] : constraints.ToArray();
    }

    public override string ToString()
    {
        return $"{ColumnName} {ValueType} {string.Join(" ", Constraints.Select(c => c.ToString()))}";
    }
}