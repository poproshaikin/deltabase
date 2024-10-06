using Utils;

namespace Db.Records;

public class RecordsReader
{
    private FileSystemManager _fs;
    private string _dbName;

    public RecordsReader(FileSystemManager fs, string dbName)
    {
        _dbName = dbName;
        _fs = fs;
    }

    public Record Read(string tableName)
    {
        Task<string[]> rows = _fs.ReadRecordRowsAsync(_dbName, tableName);
        Task<string[]> defs = _fs.ReadRecordDefsAsync(_dbName, tableName);
        
        return Record.ParseAndAwaitAsync(rows, defs, tableName);
    }

    /// <summary>
    /// Reads a last value in primary key column 
    /// </summary>
    /// <param name="recordName"></param>
    /// <returns></returns>
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