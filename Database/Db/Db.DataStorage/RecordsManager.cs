using Enums.Exceptions;
using Enums.FileSystem;
using Enums.Records.Columns;
using Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Sql.Core;
using Sql.Expressions;
using Utils;

namespace Db.DataStorage;

public class RecordsManager
{
    private FileSystemManager _fs;
    private string _dbName;

    private IMemoryCache _cache;
    private TimeSpan _cacheExpiration;
    
    private ConditionChecker _conditionChecker;
    
    private List<TableSchema> _tables;
    public IReadOnlyList<TableSchema> Tables => _tables;

    private const string records_separator = "■ ";
    
    public RecordsManager(string dbName, FileSystemManager fs)
    {
        _tables = [];
        _fs = fs;
        _dbName = dbName;
        _cache = new MemoryCache(new MemoryCacheOptions());
        _cacheExpiration = TimeSpan.FromMinutes(15);
        _conditionChecker = new ConditionChecker();
    }
    
    public void Init()
    {
        string recordsFolderPath = _fs.GetRecordsFolderPath(_dbName);
        string[] records = Directory.GetDirectories(recordsFolderPath);
        
        _tables = records.Select(ReadTableSchema).ToList();
    }
    
#region Schema access
    
    public TableSchema GetTableSchema(string tableName) =>
        Tables.FirstOrDefault(t => t.TableName == tableName) ?? throw new DbEngineException(ErrorType.TableDoesntExist);
    
#endregion
    
#region Data access
    
    public TableDataSet GetTableData(TableSchema table, int rowsLimit = int.MaxValue, string[]? columnNames = null, ConditionGroup? condition = null)
    {
        int rowsCount = ReadAndCountRows(table);
        int readingCount = Math.Min(rowsCount, rowsLimit);

        ColumnDataSet[] columnsData;

        if (columnNames is null)
        {
            columnsData = table.Columns.Select(c => GetColumnData(table, c, readingCount)).ToArray();
        }
        else
        {
            columnsData = columnNames.Select(c => GetColumnData(table, c, readingCount)).ToArray();
        }

        TableDataSet readed = new TableDataSet(table.Columns, columnsData);

        if (condition is null)
        {
            return readed;
        }
        else
        {
            return FromTableWithCondition(readed, condition);
        }
    }

    private ColumnDataSet GetColumnData(TableSchema table, string columnName, int count = int.MaxValue)
    {
        return GetColumnData(table, table.Columns.First(c => c.Name == columnName), count);
    }
    
    private ColumnDataSet GetColumnData(TableSchema table, ColumnSchema column, int count = int.MaxValue)
    {
        ColumnDataSet data;
        
        if (_cache.TryGetValue(column, out ColumnDataSet? value))
        {
            data = value!;
        }
        else
        {
            data = ReadColumnData(table, column, count);
            _cache.Set(column, data);
        }

        return data;
    }

    public RowDataSet GetRowData(TableSchema table, int rowIndex, int rowsLimit = int.MaxValue)
    {
        // получаю датасет таблицы из хеша
        // возвращаю строку из датасета
        if (_cache.TryGetValue(table, out TableDataSet? tableData))
        {
            return tableData!.Rows[rowIndex];
        }
        else
        {
            string[] data = new string[table.Columns.Length];
            
            for (int i = 0; i < table.Columns.Length; i++)
            {
                ColumnDataSet columnsData = GetColumnData(table, table.Columns[i], count: rowIndex);
                data[i] = columnsData[rowIndex];
            }

            return new RowDataSet(data);
        }
    }
    
#endregion

#region Data reading
    
    private ColumnDataSet ReadColumnData(TableSchema table, ColumnSchema column, int count = int.MaxValue)
    {
        string path = table.GetColumnPath(column);
        string content = File.ReadAllText(path);
        string[] data = content.Split(records_separator);

        return new ColumnDataSet(data);
    }
    
#endregion

#region Schema reading
    
    private TableSchema ReadTableSchema(string tableName)
    {
        string recordPath = _fs.GetRecordFolderPath(_dbName, tableName);
        ColumnSchema[] columns = ReadColumnSchemas(tableName);

        return new TableSchema(tableName, recordPath, columns);
    }
    
    private ColumnSchema[] ReadColumnSchemas(string tableName)
    {
        string[] lines = _fs.ReadRecordFile(_dbName, tableName, FileExtension.DEF);

        return lines.Select(ColumnSchema.Parse).ToArray();
    }
    
#endregion
    
    private int ReadAndCountRows(TableSchema table)
    {
        ColumnDataSet[] columns = table.Columns.Select(c => GetColumnData(table, c)).ToArray();

        return DataUtils.CountRows(columns);
    }

    private TableDataSet FromTableWithCondition(TableDataSet tableData, ConditionGroup condition)
    {
        _conditionChecker.SetTargetTable(tableData);
        _conditionChecker.SetCondition(condition);
        
        RowDataSet[] rows = tableData.Rows.Where(_conditionChecker.Check).ToArray();

        return new TableDataSet(tableData.Attributes, rows);
    }

    public bool RequiresAutoIncrement(TableSchema tableSchema) =>
        tableSchema.Columns.Any(c => c.HasConstraint(ColumnConstraint.Ai));

    public void AddRow(TableSchema tableSchema, bool requiresAutoIncrement, RowDataSet rowToInsert)
    {
        TableDataSet
    }
}

    
    // /// <summary>
    // /// Gets the index of the specified column name.
    // /// </summary>
    // /// <param name="columnName">The name of the column.</param>
    // /// <returns>The index of the column, or -1 if not found.</returns>
    // public int GetColumnId(string columnName)
    // {
    //     string[] names = Columns.Select(c => c.Name).ToArray();
    //     return Array.IndexOf(names, columnName);
    // }
    //
    // /// <summary>
    // /// Sets the default value for the specified cell in the record.
    // /// </summary>
    // /// <param name="rowId">The row index.</param>
    // /// <param name="colId">The column index.</param>
    // public void SetDefaultValue(int rowId, int colId) =>
    //     this[rowId, colId] = Columns[colId].HasConstraint(ColumnConstraint.Nn)
    //         ? string.Empty
    //         : EnumsStorage.RecordNullValue;
    //
    // /// <summary>
    // /// Parses the record definition and rows asynchronously from the provided tasks.
    // /// </summary>
    // /// <param name="rows">A task that represents the rows to parse.</param>
    // /// <param name="defs">A task that represents the definitions to parse.</param>
    // /// <param name="recordName">The name of the record.</param>
    // /// <exception cref="InvalidDataException">Thrown if the data is invalid.</exception>
    // /// <returns>A new <see cref="Record"/> instance.</returns>
    // public static Record ParseAndAwaitAsync(Task<string[]> rows, Task<string[]> defs, string recordName)
    // {
    //     var record = new Record(recordName);
    //     
    //     try
    //     {
    //         Task.WaitAll(rows, defs);
    //     }
    //     catch (DirectoryNotFoundException ex)
    //     {
    //         ThrowHelper.ThrowRecordNotFound(recordName);
    //     }
    //     
    //     record.ParseAndSetDef(defs.Result);
    //     record.ParseAndSetRows(rows.Result);
    //     return record;
    // }
    //
    // /// <summary>
    // /// Parses and sets the rows from the provided string array.
    // /// </summary>
    // /// <param name="readedRows">The rows to parse.</param>
    // /// <exception cref="InvalidDataException">Thrown if the data is invalid.</exception>
    // private void ParseAndSetRows(string[] readedRows)
    // {
    //     if (readedRows[0] == "[" && readedRows[^1] == "]")
    //     {
    //         readedRows = readedRows[1..^1];
    //     }
    //     else 
    //         throw new InvalidDataException();
    //
    //     var rows = new List<RecordRow>();
    //     rows.AddRange(readedRows.Select(s =>
    //     {
    //         string[] splitted = s.Split("■ ");
    //         if (splitted.Length != Columns.Length) throw new InvalidDataException();
    //         return new RecordRow(splitted);
    //     }));
    //
    //     _rows = rows;
    // }
    //
    // /// <summary>
    // /// Parses and sets the column definitions from the provided string array.
    // /// </summary>
    // /// <param name="def">The column definitions to parse.</param>
    // /// <exception cref="InvalidDataException">Thrown if the data is invalid.</exception>
    // private void ParseAndSetDef(string[] def)
    // {
    //     var columns = new List<RecordColumnDef>();
    //     
    //     foreach (string columnDef in def)  
    //     {
    //         string[] splitted = columnDef.Split(' ');
    //         if (splitted.Length < 2) throw new InvalidDataException();
    //
    //         string name = splitted[0];
    //         string valueType = splitted[1];
    //         string[] constraints = Array.Empty<string>();
    //         
    //         if (splitted.Length > 2)
    //         {
    //             constraints = splitted[2..];
    //         }
    //
    //         var column = new RecordColumnDef(name, valueType, constraints);
    //         columns.Add(column);
    //     }
    //
    //     Columns = columns.ToArray();
    // }
    //
    // /// <summary>
    // /// Converts the record to a byte array.
    // /// </summary>
    // /// <returns>A byte array representation of the record.</returns>
    // public byte[] ToBytes() => Encoding.UTF8.GetBytes(ToString());
    //
    // /// <summary>
    // /// Gets the data for a specified column across all rows.
    // /// </summary>
    // /// <param name="columnName">The name of the column.</param>
    // /// <returns>An array of strings containing the data for the column.</returns>
    // public string[] GetColumnData(string columnName)
    // {
    //     int columnIndex = Array.IndexOf(Columns.Select(c => c.Name).ToArray(), columnName);
    //
    //     return Rows.Select(row => row.Values[columnIndex]).ToArray();
    // }
    //
    // /// <summary>
    // /// Gets the data for a specified column at a specific row index.
    // /// </summary>
    // /// <param name="columnName">The name of the column.</param>
    // /// <param name="rowIndex">The index of the row.</param>
    // /// <returns>The value of the specified cell.</returns>
    // public string GetColumnData(string columnName, int rowIndex)
    // {
    //     int columnIndex = Array.IndexOf(Columns.Select(c => c.Name).ToArray(), columnName);
    //
    //     return Rows[rowIndex].Values[columnIndex];
    // }
    //
    // /// <summary>
    // /// Gets a subset of columns by their names.
    // /// </summary>
    // /// <param name="names">The names of the columns to retrieve.</param>
    // /// <returns>An array of <see cref="RecordRow"/> containing the specified columns.</returns>
    // public RecordRow[] GetColumns(IEnumerable<string> names) => GetColumns(names.ToArray());
    //
    // /// <summary>
    // /// Gets a subset of columns by their names.
    // /// </summary>
    // /// <param name="names">The names of the columns to retrieve.</param>
    // /// <returns>An array of <see cref="RecordRow"/> containing the specified columns.</returns>
    // public RecordRow[] GetColumns(string[] names)
    // {
    //     int[] columnsIndexes = names.Select(columnName => 
    //         Array.IndexOf(Columns, 
    //             Columns.FirstOrDefault(c => c.Name == columnName)))
    //         .ToArray();
    //
    //     return Rows.Select(r => new RecordRow(r[columnsIndexes])).ToArray();
    // }
    //
    // /// <summary>
    // /// Checks if the collection of columns contains a column with the specified name.
    // /// </summary>
    // /// <param name="columnName">The name of the column to check for.</param>
    // /// <returns>
    // /// <c>true</c> if the collection contains a column with the specified name; otherwise, <c>false</c>.
    // /// </returns>
    // public bool ContainsColumn(string columnName) => 
    //     Columns.Any(c => c.Name == columnName);
    //
    // /// <summary>
    // /// Tries to get the last primary key value from the record if primary key column exists.
    // /// </summary>
    // /// <param name="value">The value of the primary key if successful; otherwise, null.</param>
    // /// <returns>true if successful; otherwise, false.</returns>
    // public bool TryGetLastPkValue(out string value)
    // {
    //     int pkIndex = -1;
    //     
    //     for (int i = 0; i < Columns.Length; i++)
    //         if (Columns[i].HasConstraint(ColumnConstraint.Pk))
    //             pkIndex = i;
    //     try
    //     {
    //         value = Rows[^1].Values[pkIndex];
    //         return true;
    //     }
    //     catch (IndexOutOfRangeException)
    //     {
    //         value = null!;
    //         return false;
    //     }
    // }
    //
    // /// <summary>
    // /// Tries to get the last primary key value from the record if primary key column exists as an integer.
    // /// </summary>
    // /// <param name="value">The int representation of the value of the primary key if successful; otherwise, null.</param>
    // /// <returns>true if successful; otherwise, false.</returns>
    // public bool TryGetLastPkValueInt(out int? value)
    // {
    //     bool succeed = TryGetLastPkValue(out string valueString);
    //
    //     if (succeed)
    //     {
    //         value = int.Parse(valueString);
    //     }
    //     else
    //     {
    //         value = null;
    //     }
    //     
    //     return succeed;
    // }
    //
    // /// <summary>
    // /// Adds a new row to the record.
    // /// </summary>
    // /// <param name="newRow">The row to add.</param>
    // public void AddRow(RecordRow newRow)
    // {
    //     if (CheckRowValidity(newRow))
    //     {
    //         _rows.Add(newRow);
    //     }
    // }
    //
    // /// <summary>
    // /// Adds a new row to the record and orders it based on the primary key.
    // /// </summary>
    // /// <param name="newRow">The row to add.</param>
    // public void AddRowAndOrder(RecordRow newRow)
    // {
    //     if (!HasPkColumn)
    //     {
    //         AddRow(newRow);
    //     }
    //     else
    //     {
    //         int newRowPk = int.Parse(newRow[PkColumnId]);
    //
    //         if (newRowPk < _rows.Count)
    //         {
    //             if (CheckRowValidity(newRow))
    //             {
    //                 _rows.Insert(newRowPk, newRow);
    //             }
    //         }
    //         else
    //         {
    //             AddRow(newRow);
    //         }
    //     }
    // }
    //
    // /// <summary>
    // /// Deletes a row from the record by its index.
    // /// </summary>
    // /// <param name="rowId">The index of the row to delete.</param>
    // public void DeleteRow(int rowId)
    // {
    //     _rows.RemoveAt(rowId);
    // }
    //
    // /// <summary>
    // /// Checks the validity of a row to ensure it matches the expected structure.
    // /// </summary>
    // /// <param name="newRow">The row to check.</param>
    // /// <returns>true if the row is valid; otherwise, false.</returns>
    // private bool CheckRowValidity(RecordRow? newRow)
    // {
    //     if (newRow is null)
    //         return false;
    //
    //     if (newRow.Values.Length != Columns.Length)
    //         return false;
    //
    //     return true;
    // }
    //
    // /// <summary>
    // /// Clears all rows from the record.
    // /// </summary>
    // public void Clear()
    // {
    //     _rows = new List<RecordRow>();
    // }