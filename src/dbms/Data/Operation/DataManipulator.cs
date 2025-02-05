using Utils;

namespace Data.Operation;

public abstract class DataManipulator
{
    protected const long MAXIMUM_PAGE_SIZE = 8 * 1024;
    
    protected readonly string _dbName;

    protected readonly FileSystemHelper FsHelper;
    
    protected readonly DataDescriptor Descriptor;
    
    private protected readonly FileStreamPool Pool;
    
    private protected DataManipulator(string dbName,
        FileSystemHelper fsHelper,
        FileStreamPool pool,
        DataDescriptor descriptor)
    {
        _dbName = dbName;
        FsHelper = fsHelper;
        Descriptor = descriptor;
        Pool = pool;
    }
}