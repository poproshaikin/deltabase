using Enums.FileSystem;
using Utils;

namespace Db.Records;

/// <summary>
/// Provides methods to read records and their definitions from a database.
/// </summary>
public class RecordsReader
{
    private FileSystemManager _fs;
    private string _dbName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordsReader"/> class.
    /// </summary>
    /// <param name="fs">The <see cref="FileSystemManager"/> used for file operations.</param>
    /// <param name="dbName">The name of the database from which to read records.</param>
    public RecordsReader(FileSystemManager fs, string dbName)
    {
        _dbName = dbName;
        _fs = fs;
    }

    /// <summary>
    /// Reads the records from the specified table in the database.
    /// </summary>
    /// <param name="tableName">The name of the table to read records from.</param>
    /// <returns>A <see cref="Record"/> containing the rows and definitions from the specified table.</returns>
    public Record Read(string tableName)
    {
        Task<string[]> rows = _fs.ReadRecordFileAsync(_dbName, tableName, FileExtension.RECORD);
        Task<string[]> defs = _fs.ReadRecordFileAsync(_dbName, tableName, FileExtension.DEF);
        
        return Record.ParseAndAwaitAsync(rows, defs, tableName);
    }

    /// <summary>
    /// Reads the last value in the primary key column of the specified record.
    /// </summary>
    /// <param name="recordName">The name of the record to get the last primary key value from.</param>
    /// <returns>The last primary key value as a string, or null if no primary key exists.</returns>
    public string? GetRecordLastPkValue(string recordName)
    {
        Record record = Read(recordName);
        RecordColumnDef? pk = record.PkColumn;

        if (pk is null) return null;
        
        return record.GetColumnData(pk.Name)
            .Select(int.Parse)
            .Prepend(0)
            .Max()
            .ToString();
    }
}