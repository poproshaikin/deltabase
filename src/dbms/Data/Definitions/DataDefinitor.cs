using Data.Definitions.Schemes;
using Enums.FileSystem;
using Utils;

namespace Data.Definitions;

public class DataDefinitor
{
    private List<TableScheme> _tableSchemes;
    private string _dbName;
    private FileSystemHelper _fs;
    
    public DataDefinitor(string dbName, FileSystemHelper fs)
    {
        _tableSchemes = [];
        _dbName = dbName;
        _fs = fs;
        InitSchemesList();
    }
    
    public TableScheme GetTableScheme(string tableName)
    {
        return _tableSchemes.FirstOrDefault(t => t.TableName == tableName)!;
    }
    
    public bool TableExists(string tableName)
    {
        return _tableSchemes.Any(t => t.TableName == tableName);
    }

    internal TableScheme ReadTableScheme(string tableName)
    {
        string recordPath = _fs.GetRecordFolderPath(_dbName, tableName);
        ColumnScheme[] columns = ReadColumnSchemes(tableName);

        return new TableScheme(tableName, columns);
    }

    internal ColumnScheme[] ReadColumnSchemes(string tableName)
    {
        string[] lines = _fs.ReadRecordFile(_dbName, tableName, FileExtension.DEF);

        return lines.Select(ColumnScheme.Parse).ToArray();
    }

    private void InitSchemesList()
    {
        string recordsFolderPath = _fs.GetRecordsFolderPath(_dbName);
        string[] records = Directory.GetDirectories(recordsFolderPath).Select(p => p.Split("\\").Last()).ToArray();
        
        _tableSchemes = records.Select(ReadTableScheme).ToList();
    }
}