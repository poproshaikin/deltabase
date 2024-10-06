using Sql.Queries;
using Utils;

namespace Sql.Core;

public class QueryProcessor
{
    private FileSystemManager _fs;
    private string _dbName;

    public QueryProcessor(FileSystemManager fs, string dbName)
    {
        _fs = fs;
        _dbName = dbName;
    }

    public byte[] ExecuteDql(SqlQuery query)
    {
        QueryEvaluator evaluator = new();
        
    }
    
    public byte[] ExecuteDml(DmlQuery query)
    {
        throw new NotImplementedException();
    }
    
    // private byte[] ExecuteReader(SelectQuery select)
    // {
    //     RecordsReader reader = new(_fs, dbName: Name);
    //     Record read = reader.Read(select.TableName);
    //     
    //     if (select.Condition is null)
    //     {
    //         if (select.SelectAllColumns)
    //         {
    //             return read.ToBytes();
    //         }
    //         else
    //         {
    //             SqlToken[] columnNames = select.PassedColumns;
    //             return new Record(select.TableName, read.GetColumns(columnNames.Select(t => t.Value)))
    //                 .ToBytes();
    //         }
    //     }
    //     else
    //     {
    //         ConditionChecker checker = new(read, select.Condition);
    //         RecordRow[] filteredRows = read.Rows.Where(row => checker.IsMet(row)).ToArray();
    //         
    //         if (select.SelectAllColumns)
    //         {
    //             return new Record(select.TableName, filteredRows)
    //                 .ToBytes();
    //         }
    //         else
    //         {
    //             RecordRow[] rowsToReturn = _qp.GetValuesFromRowsAtColumns(sourceColumnNames: read.Columns, columnNamesToGetFrom: select.PassedColumns,
    //                 filteredRows);
    //             
    //             return new Record(select.TableName, rowsToReturn)
    //                 .ToBytes();
    //         }
    //     }
    // }
    //
    // private byte[] ExecuteInsert(InsertQuery insert)
    // {
    //     RecordsReader reader = new(_fs, dbName: Name);
    //     Record read = reader.Read(insert.TableName);
    //     if (read.HasPkColumn)
    //     {
    //         bool needsAutoIncrement = _qp.NeedsAutoIncrement(read, insert);
    //         if (needsAutoIncrement)
    //         {
    //             _qp.AutoIncrement(read, insert);
    //         }
    //         else if (!_qp.IsPassedPkValueValid(read, insert))
    //         {
    //             return ParseHelper.GetBytes(TcpResponseType.PassedPkValueIsntUnique);
    //         }
    //     }
    //
    //     return SortAndWrite(read, insert);
    // }
    //
    // private byte[] SortAndWrite(Record read, InsertQuery insert)
    // {
    //     RecordsWriter writer = new(_fs, dbName: Name);
    //     
    //     string[] valuesInRightOrder = _qp.SortValues(read.Columns, columnsToSort_t: insert.PassedColumns,
    //         valuesToSort_t: insert.PassedValues);
    //     
    //     RecordRow rowToWrite = new RecordRow(valuesInRightOrder);
    //     writer.Write(read, newRow: rowToWrite);
    //     
    //     return ParseHelper.GetBytes(TcpResponseType.Success);
    // }
    //
    // private byte[] ExecuteUpdate(UpdateQuery update)
    // {
    //     RecordsReader reader = new(_fs, dbName: Name);
    //     Record read = reader.Read(update.TableName);
    //
    //     ConditionChecker? checker = update.Condition is not null ? new ConditionChecker(read, update.Condition) : null;
    //
    //     foreach (AssignExpr assignment in update.Assignments)
    //     {
    //         string columnName = assignment.LeftOperand;
    //         int columnId = read.GetColumnId(columnName);
    //
    //         if (!_qp.IsPassedValueTypeValid(assignment.RightOperand, read.Columns[columnId]))
    //             throw new InvalidDataException();
    //         
    //         foreach (RecordRow row in read.Rows)
    //         {
    //             if (checker is not null && !checker.IsMet(row)) 
    //                 continue;
    //             
    //             row[columnId] = assignment.RightOperand;
    //         }
    //     }
    //
    //     RecordsWriter writer = new(_fs, dbName: Name);
    //     writer.Write(read);
    //
    //     return ParseHelper.GetBytes(TcpResponseType.Success);
    // }
    //
    // private byte[] ExecuteDelete(DeleteQuery delete)
    // {
    //     // DELETE * FROM Students WHERE Id == 1
    //     
    //     RecordsReader reader = new(_fs, dbName: Name);
    //     RecordsWriter writer = new(_fs, dbName: Name);
    //     Record read = reader.Read(delete.TableName);
    //
    //     ConditionChecker? checker = null;
    //     if (delete.Condition is not null)
    //         checker = new ConditionChecker(sourceRecord: read, delete.Condition);
    //     
    //     int[] columnIds = delete.PassedColumns
    //         .Select(t => read.GetColumnId(t))
    //         .ToArray();
    //
    //     for (int rowId = 0; rowId < read.Rows.Length; rowId++)
    //     {
    //         if (checker is not null && !checker.IsMet(read.Rows[rowId])) 
    //             continue;
    //
    //         if (delete.SelectAllColumns)
    //         {
    //             read.DeleteRow(rowId);
    //         }
    //         else
    //         {
    //             foreach (int colId in columnIds)
    //             {
    //                 read.SetDefaultValue(rowId, colId);
    //             }
    //         }
    //     }
    //
    //     writer.Write(read);
    //     return ParseHelper.GetBytes(TcpResponseType.Success);
    // }
}