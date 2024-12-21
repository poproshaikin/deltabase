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


    private bool _encoding => _encoder is not null;
    
    private IDataEncoder? _encoder;
    
    public DataServiceProvider(string dbName, FileSystemHelper fs, FileAccess poolAccess)
    {
        _dbName = dbName;
        _fs = fs;
        _pool = new FileStreamPool(poolAccess);
    }

    public void SetEncoding(IDataEncoder? encoder)
    {
        _encoder = encoder;
    }

    public DataDefinitor CreateDefinitor()
    {
        return new DataDefinitor(_dbName, _fs);
    }

    public DataReader CreateReader()
    {
        return new DataReader(_dbName, _fs, _pool, CreateDefinitor(), _encoder);
    }

    public DataWriter CreateInserter()
    {
        return new DataWriter(_dbName, _fs, _pool, CreateDefinitor(), _encoder);
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