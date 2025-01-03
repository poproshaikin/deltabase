using Enums.Records.Columns;

namespace Sql.Executing.App;

internal class TypeChecker
{
    internal bool Matches(SqlValueType columnType, string value)
    {
        return columnType switch
        {
            SqlValueType.String => true,

            SqlValueType.Integer => int.TryParse(value, out _),
            SqlValueType.Float => float.TryParse(value, out _),
            SqlValueType.Char => char.TryParse(value, out _),

            // TODO доделать остальные типы
            _ => throw new NotImplementedException()
        };
    }
}