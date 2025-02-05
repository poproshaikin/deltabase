// using Enums.Records.Columns;
//
// namespace Enums;
//
// public static class EnumHelper
// {
//     // public static SqlValueType ConvertValueType(string valueTypeStr)
//     // {
//     //     return valueTypeStr switch
//     //     {
//     //         "INTEGER" => SqlValueType.Integer,
//     //         "STRING" => SqlValueType.String,
//     //         "CHAR" => SqlValueType.Char,
//     //         "FLOAT" => SqlValueType.Float,
//     //     
//     //         "INTEGER[]" => SqlValueType.IntegerArr,
//     //         "STRING[]" => SqlValueType.StringArr,
//     //         "CHAR[]" => SqlValueType.CharArr,
//     //         "FLOAT[]" => SqlValueType.FloatArr,
//     //
//     //         _ => throw new ArgumentOutOfRangeException(nameof(valueTypeStr), valueTypeStr, null)
//     //     };
//     // }
//
//     public static ColumnConstraint ConvertConstraint(string constraintStr)
//     {
//         return constraintStr switch
//         {
//             "PK" => ColumnConstraint.Pk,
//             "UN" => ColumnConstraint.Un,
//             "NN" => ColumnConstraint.Nn,
//             "AI" => ColumnConstraint.Ai,
//
//             _ => throw new ArgumentOutOfRangeException(nameof(constraintStr), constraintStr, null)
//         };
//     }
// }