// using Data.Definitions.Schemes;
// using Data.Models;
// using Enums.Exceptions;
// using Enums.Records.Columns;
// using Exceptions;
//
// namespace Data.Operation.IO;
//
// public class BinaryRowIO : BinaryDataIO
// {
//     public BinaryRowIO(FileStream stream) : base(stream)
//     {
//     }
//
//     public BinaryRowIO(FileStream stream, TableScheme scheme) : base(stream, scheme)
//     {
//     }
//     
//     public async Task<PageRow> ReadRowAsync()
//     {
//         return await ReadRowAsync(_scheme ?? throw new DbEngineException(ErrorType.InternalServerError));
//     }
//     
//     public async Task<PageRow> ReadRowAsync(TableScheme scheme)
//     {
//         int valuesCount = await ReadIntAsync();
//         int rid = await ReadIntAsync();
//         
//         object[] values = new object[valuesCount];
//         for (int i = 0; i < valuesCount; i++)
//         {
//             SqlValueType valueType = scheme.Columns[i].ValueType;
//             
//             if (valueType == SqlValueType.Integer) 
//                 values[i] = await ReadIntAsync();
//             
//             if (valueType == SqlValueType.String)
//                 values[i] = await ReadStringAsync();
//             
//             if (valueType == SqlValueType.Char)
//                 values[i] = await ReadCharAsync();
//             
//             if (valueType == SqlValueType.Float) 
//                 values[i] = await ReadFloatAsync();
//
//             if (valueType == SqlValueType.Boolean)
//                 values[i] = await ReadBooleanAsync();
//         }
//
//         return new PageRow(rid, values.ToArray());
//     }
//
//     public async Task WriteRowAsync(PageRow row)
//     {
//         await WriteRowAsync(row, _scheme ?? throw new DbEngineException(ErrorType.InternalServerError));
//     }
//     
//     public async Task WriteRowAsync(PageRow row, TableScheme scheme)
//     {
//         int valuesCount = row.ValuesCount;
//         await WriteIntAsync(valuesCount);
//         await WriteIntAsync(row.RId);
//         
//         for (int i = 0; i < valuesCount; i++)
//         {
//             SqlValueType valueType = scheme.Columns[i].ValueType;
//
//             if (valueType == SqlValueType.Integer)
//                 await WriteIntAsync((int)row[i]);
//             
//             if (valueType == SqlValueType.String)
//                 await WriteStringAsync((string)row[i]);
//             
//             if (valueType == SqlValueType.Char)
//                 await WriteCharAsync((char)row[i]);
//             
//             if (valueType == SqlValueType.Float) 
//                 await WriteFloatAsync((float)row[i]);
//
//             if (valueType == SqlValueType.Boolean)
//                 await WriteBooleanAsync((bool)row[i]);
//         }
//     }
// }