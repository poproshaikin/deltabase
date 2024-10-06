using Enums.Records.Columns;

namespace Enums;

public static class EnumHelper
{
    public static ColumnValueType ConvertValueType(string valueTypeStr)
    {
        return valueTypeStr switch
        {
            "INTEGER" => ColumnValueType.Integer,
            "STRING" => ColumnValueType.String,
            "CHAR" => ColumnValueType.Char,
            "FLOAT" => ColumnValueType.Float,
        
            "INTEGER[]" => ColumnValueType.IntegerArr,
            "STRING[]" => ColumnValueType.StringArr,
            "CHAR[]" => ColumnValueType.CharArr,
            "FLOAT[]" => ColumnValueType.FloatArr,

            _ => throw new ArgumentOutOfRangeException(nameof(valueTypeStr), valueTypeStr, null)
        };
    }

    public static ColumnConstraint ConvertConstraint(string constraintStr)
    {
        return constraintStr switch
        {
            "PK" => ColumnConstraint.Pk,
            "UN" => ColumnConstraint.Un,
            "NN" => ColumnConstraint.Nn,
            "AU" => ColumnConstraint.Au,

            _ => throw new ArgumentOutOfRangeException(nameof(constraintStr), constraintStr, null)
        };
    }
}

public enum CommandContentType
{
    Unspecified = 0,
    Command = 1,
    Data = 2
}