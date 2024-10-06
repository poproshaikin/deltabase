using System.Reflection;
using System.Runtime.CompilerServices;
using Enums.FileSystem;

namespace Utils;

public class FileSystemManager
{
    public string ServerName { get; private set; }
    
    private string? _serverPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    
    public FileSystemManager(string serverName)
    {
        ServerName = serverName;
    }

    /// <summary>
    /// Asynchronously reads the record file from the database file system with <see cref="RECORD_EXTENSION"/>
    /// </summary>
    /// <param name="dbName">The name of the database accessing to the file system</param>
    /// <param name="recordName">The name of the record readed</param>
    public async Task<string[]> ReadRecordRowsAsync(string dbName, string recordName)
    {
        string path = GetRecordPath(dbName, recordName, FileExtension.RECORD);
        return await File.ReadAllLinesAsync(path);
    }

    /// <summary>
    /// Asynchronously reads the record file from the database file system with <see cref="DEF_EXTENSION"/>
    /// </summary>
    /// <param name="dbName">The name of the database accessing to the file system</param>
    /// <param name="recordName">The name of the record readed</param>
    public async Task<string[]> ReadRecordDefsAsync(string dbName, string recordName)
    {
        string path = GetRecordPath(dbName, recordName, FileExtension.DEF);
        return await File.ReadAllLinesAsync(path);
    }
    
    public async Task WriteRecordAsync(string dbName, string recordName, IEnumerable<string> rowsList)
    {
        string path = GetRecordPath(dbName, recordName, FileExtension.RECORD);
        await File.WriteAllLinesAsync(path, rowsList);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDatabaseFolderPath(string databaseName)
    {
        return $"{_serverPath}/{ServerName}/db/{databaseName}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDatabaseConfPath(string databaseName)
    {
        return $"{GetDatabaseFolderPath(databaseName)}/{databaseName}.{ParseHelper.ParseExtension(FileExtension.CONF)}";
    }

    public string GetRecordPath(string databaseName, string recordName, FileExtension extension)
    {
        string extensionStr = extension == default ? "" : $".{ParseHelper.ParseExtension(extension)}";
        return $"{GetDatabaseFolderPath(databaseName)}/records/{recordName}/{recordName}{extensionStr}";
    }
}