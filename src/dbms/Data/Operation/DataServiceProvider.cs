using Data.Definitions;
using Data.Operation.IO;
using Utils;

namespace Data.Operation;

public class DataServiceProvider
{
    private string _dbName;
    
    private FileSystemHelper _fs;
    
    private PageController _pageController;
    
    public DataServiceProvider(string dbName, FileSystemHelper fs)
    {
        _dbName = dbName;
        _fs = fs;
        _pageController = new PageController(dbName, fs);
    }

    public DataDescriptor CreateDescriptor()
    {
        return new DataDescriptor(_dbName, _fs);
    }

    public DataScanner CreateScanner()
    {
        return new DataScanner(_dbName, _fs, _pageController, CreateDescriptor());
    }

    public DataInserter CreateInserter()
    {
        return new DataInserter(_dbName, _fs, _pageController, CreateDescriptor());
    }

    public DataSorter CreateSorter()
    {
        return new DataSorter(_dbName, _fs, _pageController, CreateDescriptor());
    }

    // internal FileStreamPool GetPool()
    // {
    //     return _pool;
    // }
    //
    // public void SetStreamAccess(FileAccess access, string tableName)
    // {
    //     FileInfo? file = _pool.Pool.Keys.FirstOrDefault(f => f.Name == tableName);
    //
    //     SetStreamAccess(access, file);
    // }
    //
    // public void SetStreamAccess(FileAccess access, FileInfo? file)
    // {
    //     ArgumentNullException.ThrowIfNull(file);
    //     
    //     _pool.ChangeAccess(access, file);
    // }
}