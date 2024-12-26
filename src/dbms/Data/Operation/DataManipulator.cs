using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Models;
using Data.Operation.IO;
using Enums.Records.Columns;
using Exceptions;
using Utils;

namespace Data.Operation;

public abstract class DataManipulator
{
    protected string _dbName;

    protected FileSystemHelper _fs;
    
    protected DataDefinitor _definitor;
    
    protected FileStreamPool _pool;
    
    private protected DataManipulator(string dbName,
        FileSystemHelper fs,
        FileStreamPool pool,
        DataDefinitor definitor)
    {
        _dbName = dbName;
        _fs = fs;
        _definitor = definitor;
        _pool = pool;
    }
}