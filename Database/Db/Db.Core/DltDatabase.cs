using Db.Records;
using Enums.FileSystem;
using Enums.Tcp;
using Sql.Core;
using Sql.Core.Validation;
using Sql.Expressions;
using Sql.Queries;
using Sql.Tokens;
using Utils;

namespace Db.Core;

/// <summary>
/// Represents a database that processes SQL queries.
/// </summary>
public class DltDatabase
{
    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public string Name { get; private set; }

    private QueryParser _parser;
    private QueryHelper _helper;
    private QueryValidator _validator;
    private FileSystemManager _fs;

    /// <summary>
    /// Initializes a new instance of the <see cref="DltDatabase"/> class with the specified database name and file system manager.
    /// </summary>
    /// <param name="dbName">The name of the database.</param>
    /// <param name="fs">The file system manager used for managing database files.</param>
    public DltDatabase(string dbName, FileSystemManager fs)
    {
        _fs = fs;
        _parser = new QueryParser();
        _helper = new QueryHelper();
        _validator = new QueryValidator();
        Name = dbName;
    }

    /// <summary>
    /// Executes the given SQL query and returns the result as a byte array.
    /// </summary>
    /// <param name="sql">The SQL query to be executed.</param>
    /// <returns>A byte array containing the result of the query.</returns>
    public byte[] ExecuteRequest(string sql)
    {
        SqlQuery command = _parser.Parse(sql);
        return ExecuteQuery(command);
    }
    
    /// <summary>
    /// Executes a given SQL command based on its type.
    /// </summary>
    /// <param name="command">The SQL command to be executed.</param>
    /// <returns>Response bytes based on the execution result.</returns>
    /// <exception cref="NotImplementedException">Thrown if the command type is not implemented or recognized.</exception>
    private byte[] ExecuteQuery(SqlQuery command)
    {
        try
        {
            if (_validator.IsInvalid(command, out ResponseType error))
            {
                return ConvertHelper.GetBytes(error);
            }
            
            return command switch
            {
                SelectQuery select => ExecuteReader(select),
                InsertQuery insert => ExecuteInsert(insert),
                UpdateQuery update => ExecuteUpdate(update),
                DeleteQuery delete => ExecuteDelete(delete),
                CreateTableQuery create => ExecuteCreate(create),

                _ => throw new NotImplementedException()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return ConvertHelper.GetBytes(ResponseType.InternalServerError);
        }
    }

    /// <summary>
    /// Executes a SELECT query and retrieves records based on the specified criteria.
    /// </summary>
    /// <param name="select">The select query containing table name, columns, and conditions.</param>
    /// <returns>The retrieved records in byte format.</returns>
    private byte[] ExecuteReader(SelectQuery select)
    {
        RecordsReader reader = new(_fs, dbName: Name);
        Record read = reader.Read(select.TableName);

        if (!_fs.ExistsRecord(dbName: Name, select.TableName))
        {
            return ConvertHelper.GetBytes(ResponseType.TableDoesntExist);
        }
        
        if (select.Condition is null)
        {
            if (select.SelectAllColumns)
            {
                return read.ToBytes();
            }
            else
            {
                SqlToken[] columnNames = select.PassedColumns;
                return new Record(select.TableName, read.GetColumns(columnNames.Select(t => t.Value)))
                    .ToBytes();
            }
        }
        else
        {
            ConditionChecker checker = new(read, select.Condition);
            RecordRow[] filteredRows = read.Rows.Where(row => checker.IsMet(row)).ToArray();
            
            if (select.SelectAllColumns)
            {
                return new Record(select.TableName, filteredRows)
                    .ToBytes();
            }
            else
            {
                RecordRow[] rowsToReturn = _helper.GetValuesFromRowsAtColumns(sourceColumnNames: read.Columns, columnNamesToGetFrom: select.PassedColumns,
                    filteredRows);
                
                return new Record(select.TableName, rowsToReturn)
                    .ToBytes();
            }
        }
    }
    
    /// <summary>
    /// Inserts a new row into the specified table.
    /// </summary>
    /// <param name="insert">The insert query containing the table name and values to be inserted.</param>
    /// <returns>Response indicating success or an error message if the insertion fails.</returns>
    /// <exception cref="ResponseType">Returns <see cref="ResponseType.PassedPkValueIsntUnique"/> if the primary key value is not unique.</exception>
    private byte[] ExecuteInsert(InsertQuery insert)
    {
        RecordsReader reader = new(_fs, dbName: Name);
        Record read = reader.Read(insert.TableName);
        if (read.HasPkColumn)
        {
            bool needsAutoIncrement = _helper.NeedsAutoIncrement(read, insert);
            if (needsAutoIncrement)
            {
                _helper.AutoIncrement(read, insert);
            }
            else if (!_helper.IsPassedPkValueValid(read, insert))
            {
                return ConvertHelper.GetBytes(ResponseType.PassedPkValueIsntUnique);
            }
        }

        return SortAndWrite(read, insert);
    }

    /// <summary>
    /// Sorts values according to the specified columns and writes the new row to the database.
    /// </summary>
    /// <param name="read">The current records of the table.</param>
    /// <param name="insert">The insert query containing columns and values to be sorted and inserted.</param>
    /// <returns>Response indicating success.</returns>
    private byte[] SortAndWrite(Record read, InsertQuery insert)
    {
        RecordsWriter writer = new(_fs, dbName: Name);
        
        string[] valuesInRightOrder = _helper.SortValues(read.Columns, columnsToSort_t: insert.PassedColumns,
            valuesToSort_t: insert.PassedValues);
        
        RecordRow rowToWrite = new RecordRow(valuesInRightOrder);
        writer.Write(read, newRow: rowToWrite);
        
        return ConvertHelper.GetBytes(ResponseType.Success);
    }

    /// <summary>
    /// Updates existing rows in the specified table based on provided assignments and conditions.
    /// </summary>
    /// <param name="update">The update query containing table name, assignments, and conditions.</param>
    /// <returns>Response indicating success.</returns>
    /// <exception cref="ResponseType">Returns <see cref="ResponseType.InvalidPassedValueType"/> if the value type for an assignment is invalid.</exception>
    private byte[] ExecuteUpdate(UpdateQuery update)
    {
        RecordsReader reader = new(_fs, dbName: Name);
        Record read = reader.Read(update.TableName);

        ConditionChecker? checker = update.Condition is not null ? new ConditionChecker(read, update.Condition) : null;

        foreach (AssignExpr assignment in update.Assignments)
        {
            string columnName = assignment.LeftOperand;
            int columnId = read.GetColumnId(columnName);

            if (!_helper.IsPassedValueTypeValid(assignment.RightOperand, read.Columns[columnId]))
                return ConvertHelper.GetBytes(ResponseType.InvalidPassedValueType);
            
            foreach (RecordRow row in read.Rows)
            {
                if (checker is not null && !checker.IsMet(row)) 
                    continue;
                
                row[columnId] = assignment.RightOperand;
            }
        }

        RecordsWriter writer = new(_fs, dbName: Name);
        writer.Write(read);

        return ConvertHelper.GetBytes(ResponseType.Success);
    }
    
    /// <summary>
    /// Deletes records from the specified table based on conditions.
    /// </summary>
    /// <param name="delete">The delete query containing table name, columns, and conditions.</param>
    /// <returns>Response indicating success.</returns>
    private byte[] ExecuteDelete(DeleteQuery delete)
    {
        RecordsReader reader = new(_fs, dbName: Name);
        RecordsWriter writer = new(_fs, dbName: Name);
        Record read = reader.Read(delete.TableName);

        ConditionChecker? checker = null;
        if (delete.Condition is not null)
            checker = new ConditionChecker(sourceRecord: read, delete.Condition);
        
        int[] columnIds = delete.PassedColumns
            .Select(t => read.GetColumnId(t))
            .ToArray();

        for (int rowId = 0; rowId < read.Rows.Length; rowId++)
        {
            if (checker is not null && !checker.IsMet(read.Rows[rowId])) 
                continue;

            if (delete.SelectAllColumns)
            {
                read.DeleteRow(rowId);
            }
            else
            {
                foreach (int colId in columnIds)
                {
                    read.SetDefaultValue(rowId, colId);
                }
            }
        }

        writer.Write(read);
        return ConvertHelper.GetBytes(ResponseType.Success);
    }
    
    private byte[] ExecuteCreate(CreateTableQuery create)
    {
        if (_fs.ExistsRecord(dbName: Name, create.TableName))
        {
            return ConvertHelper.GetBytes(ResponseType.TableAlreadyExists);
        }

        _fs.CreateRecordFolder(dbName: Name, create.TableName);

        string[] defRows = create.NewColumns.Select(c => c.ToString()!).ToArray();
        string[] recordRows = ["[", "]"];
        Task.WhenAll(
            _fs.WriteToRecordFileAsync(dbName: Name, create.TableName, FileExtension.DEF, defRows),
            _fs.WriteToRecordFileAsync(dbName: Name, create.TableName, FileExtension.RECORD, recordRows)
        );
        
        return ConvertHelper.GetBytes(ResponseType.Success);
    }
}