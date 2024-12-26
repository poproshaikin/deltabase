using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Enums;
using Enums.FileSystem;
using Utils.Settings;

namespace Utils;

/// <summary>
/// Manages file system operations for databases and records within a server's file system.
/// </summary>
public class FileSystemHelper
{
    /// <summary>
    /// Gets the name of the server managing the file system.
    /// </summary>
    public string ServerName { get; private set; }

    /// <summary>
    /// Gets the base server path of the executing assembly's location.
    /// </summary>
    private string _serverPath;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemHelper"/> class.
    /// </summary>
    /// <param name="serverName">The name of the server managing the file system.</param>
    public FileSystemHelper(string serverName)
    {
        ServerName = serverName;
        _serverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    }

    public FileSystemHelper(string serverName, string serverPath)
    {
        ServerName = serverName;
        _serverPath = serverPath;
    }
    
    /// <summary>
    /// Gets the full file system path to the folder of the specified database.
    /// </summary>
    /// <param name="databaseName">The name of the database.</param>
    /// <returns>The full path to the database folder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDatabaseFolderPath(string databaseName)
    {
        return $"{_serverPath}/{ServerName}/db/{databaseName}";
    }
    
    public bool ExistsDatabase(string databaseName)
    {
        return Directory.Exists($"{_serverPath}/{ServerName}/db/{databaseName}");
    }
    
    /// <summary>
    /// Gets the full file system path to the configuration file of the specified database.
    /// </summary>
    /// <param name="databaseName">The name of the database.</param>
    /// <returns>The full path to the database configuration file.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDatabaseConfPath(string databaseName)
    {
        return $"{GetDatabaseFolderPath(databaseName)}/{databaseName}.{EnumsStorage.GetExtensionString(FileExtension.CONF)}";
    }

    /// <summary>
    /// Gets the full file system path to the specified record file within a database.
    /// </summary>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="recordName">The name of the record.</param>
    /// <param name="extension">The file extension for the record (e.g., RECORD or DEF).</param>
    /// <returns>The full path to the record file.</returns>
    public string GetRecordFilePath(string databaseName, string recordName, FileExtension extension)
    {
        string extensionStr = extension == default ? "" : $".{EnumsStorage.GetExtensionString(extension)}";
        return $"{GetDatabaseFolderPath(databaseName)}/records/{recordName}/{recordName}{extensionStr}";
    }
    
    /// <summary>
    /// Reads the specified record file asynchronously and returns its contents as an array of strings.
    /// </summary>
    /// <param name="dbName">The name of the database.</param>
    /// <param name="recordName">The name of the record file.</param>
    /// <param name="extension">The file extension of the record.</param>
    /// <returns>An array of strings representing the contents of the file.</returns>
    public async Task<string[]> ReadRecordFileAsync(string dbName, string recordName, FileExtension extension)
    {
        string path = GetRecordFilePath(dbName, recordName, extension);
        return await File.ReadAllLinesAsync(path);
    }
    
    public async Task<string[]> ReadRecordFileAsync(string dbName, string recordName, string columnName)
    {
        string path =
            $"{_serverPath}/{ServerName}/db/{dbName}/records/{recordName}/{columnName}.{EnumsStorage.GetExtensionString(FileExtension.RECORD)}";
        return await File.ReadAllLinesAsync(path);
    }
    
    /// <summary>
    /// Reads the specified record file and returns its contents as an array of strings.
    /// </summary>
    /// <param name="dbName">The name of the database.</param>
    /// <param name="recordName">The name of the record file.</param>
    /// <param name="extension">The file extension of the record.</param>
    /// <returns>An array of strings representing the contents of the file.</returns>
    public string[] ReadRecordFile(string dbName, string recordName, FileExtension extension) =>
        ReadRecordFileAsync(dbName, recordName, extension).GetAwaiter().GetResult();

    /// <summary>
    /// Writes the specified rows to the record file asynchronously.
    /// </summary>
    /// <param name="dbName">The name of the database.</param>
    /// <param name="recordName">The name of the record file.</param>
    /// <param name="extension">The file extension of the record.</param>
    /// <param name="rows">An enumerable collection of strings representing the rows to write to the file.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public async Task WriteToRecordFileAsync(string dbName, string recordName, FileExtension extension, IEnumerable<string> rows)
    {
        string path = GetRecordFilePath(dbName, recordName, extension);
        await File.WriteAllLinesAsync(path, rows);
    }
    
    /// <summary>
    /// Writes the specified rows to the record file.
    /// </summary>
    /// <param name="dbName">The name of the database.</param>
    /// <param name="recordName">The name of the record file.</param>
    /// <param name="extension">The file extension of the record.</param>
    /// <param name="rows">An enumerable collection of strings representing the rows to write to the file.</param>
    public void WriteToRecordFile(string dbName, string recordName, FileExtension extension, IEnumerable<string> rows) =>
        WriteToRecordFileAsync(dbName, recordName, extension, rows).GetAwaiter().GetResult();

    /// <summary>
    /// Gets the folder path of the specified record in the given database.
    /// </summary>
    /// <param name="dbName">The name of the database.</param>
    /// <param name="recordName">The name of the record.</param>
    /// <returns>The full folder path of the record within the database.</returns>
    public string GetRecordFolderPath(string dbName, string recordName)
    {
        string dbFolderPath = GetDatabaseFolderPath(dbName);
        return $"{dbFolderPath}/records/{recordName}";
    }

    /// <summary>
    /// Checks whether a specific table exists in the database.
    /// </summary>
    /// <param name="dbName">The name of the database being checked.</param>
    /// <param name="recordName">The name of the table to check for existence.</param>
    /// <returns><c>true</c> if the table exists; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ExistsRecord(string dbName, string recordName)
    {
        return Directory.Exists(GetRecordFolderPath(dbName, recordName));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CreateRecordFolder(string dbName, string recordName)
    {
        string path = GetRecordFolderPath(dbName, recordName);
        Directory.CreateDirectory(path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CreateServerFolder()
    {
        string path = $"{_serverPath}/{ServerName}";
        Directory.CreateDirectory(path);
    }

    public ServerSettings GetServerSettings()
    {
        return SettingsHelper.Parse(File.ReadAllText(GetServerConfPath()))
            .ToServerSettings();
    }
    
    public DatabaseSettings GetDatabaseSettings(string dbName)
    {
        return SettingsHelper.Parse(File.ReadAllText(GetDatabaseConfPath(dbName)))
            .ToDatabaseSettings();
    }

    public void CreateServerConfigFile(ushort port, string password)
    {
        string hashedPassword = ConvertHelper.Sha256(password);
        string path = $"{_serverPath}/{ServerName}.{EnumsStorage.GetExtensionString(FileExtension.CONF)}";
        File.WriteAllLines(path, [ServerName, port.ToString(), hashedPassword]);
    }
    
    public string GetServerConfPath()
    {
        return $"{ServerName}/{ServerName}.conf";
    }

    public void CreateDbsFile()
    {
        string path = $"{_serverPath}/{ServerName}/dbs";
        File.Create(path);
    }

    public void CreateDbInServerFolder()
    {
        string path = $"{_serverPath}/{ServerName}/db/";
        Directory.CreateDirectory(path);
    }

    public void CreateDbFolder(string dbName)
    {
        string path = $"{_serverPath}/{ServerName}/db/{dbName}";
        Directory.CreateDirectory(path);
    }

    public async Task CreateDbConfigFileAsync(string dbName)
    {
        string path = $"{_serverPath}/{ServerName}/db/{dbName}/{dbName}.conf";
        await File.WriteAllTextAsync(path, dbName);
    }

    public void CreateRecordsFolder(string dbName)
    {
        string path = $"{_serverPath}/{ServerName}/db/{dbName}/records";
        Directory.CreateDirectory(path);
    }

    public string GetRecordsFolderPath(string dbName) => $"{_serverPath}/{ServerName}/db/{dbName}/records";

    public string ReadServerConfFile()
    {
        string path = GetServerConfPath();
        return File.ReadAllText(path);
    }
}