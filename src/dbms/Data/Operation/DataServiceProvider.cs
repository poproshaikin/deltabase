using Data.Definitions;
using Data.Encoding;
using Data.Operation.IO;
using Utils;

namespace Data.Operation;

public class DataServiceProvider
{
    private string _dbName;
    
    private FileSystemHelper _fs;

    private FileStreamPool _pool;
    
    public DataServiceProvider(string dbName, FileSystemHelper fs, FileAccess poolAccess)
    {
        _dbName = dbName;
        _fs = fs;
        _pool = new FileStreamPool(poolAccess);
    }

    public DataDefinitor CreateDefinitor()
    {
        return new DataDefinitor(_dbName, _fs);
    }

    public DataScanner CreateReader()
    {
        return new DataScanner(_dbName, _fs, _pool, CreateDefinitor());
    }

    public DataInserter CreateInserter()
    {
        return new DataInserter(_dbName, _fs, _pool, CreateDefinitor());
    }

    public DataSorter CreateSorter()
    {
        return new DataSorter(_dbName, _fs, _pool, CreateDefinitor());
    }

    public FileStreamPool GetPool()
    {
        return _pool;
    }

    public void SetStreamAccess(FileAccess access, string tableName)
    {
        FileInfo? file = _pool.Pool.Keys.FirstOrDefault(f => f.Name == tableName);

        SetStreamAccess(access, file);
    }
    
    public void SetStreamAccess(FileAccess access, FileInfo? file)
    {
        ArgumentNullException.ThrowIfNull(file);
        
        _pool.ChangeAccess(access, file);
    }
}