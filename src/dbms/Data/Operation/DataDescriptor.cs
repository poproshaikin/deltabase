using Data.Definitions.Schemes;
using Enums;
using Enums.FileSystem;
using Utils;

namespace Data.Operation;

// TODO просто привести в порядок, отрефакторить что бы прилично выглядело
public class DataDescriptor
{
    private List<TableScheme> _tableSchemes;
    private string _dbName;
    private FileSystemHelper _fs;
    
    public DataDescriptor(string dbName, FileSystemHelper fs)
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
        string[] lines = _fs.ReadRecordFile(_dbName, tableName, FileType.Def);

        return lines.Select(ColumnScheme.Parse).ToArray();
    }

    internal DirectoryInfo GetTableDirectory(string tableName)
    {
        string path = _fs.GetRecordFolderPath(_dbName, tableName);
        return new DirectoryInfo(path);
    }
    
    internal FileInfo[] GetTableFiles(string tableName, FileType fileType)
    {
        return GetTableDirectory(tableName)
            .GetFiles(searchPattern: $"*.{EnumsStorage.GetExtensionString(fileType)}");
    }

    private void InitSchemesList()
    {
        string recordsFolderPath = _fs.GetRecordsFolderPath(_dbName);
        string[] records = Directory.GetDirectories(recordsFolderPath).Select(p => p.Split("\\").Last()).ToArray();
        
        _tableSchemes = records.Select(ReadTableScheme).ToList();
    }
}