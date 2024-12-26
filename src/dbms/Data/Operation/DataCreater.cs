using Data.Definitions;
using Data.Operation.IO;
using Utils;

namespace Data.Operation;

public class DataCreater : DataManipulator
{
    internal DataCreater(string dbName,
        FileSystemHelper fs,
        FileStreamPool pool,
        DataDefinitor definitor) : base(dbName,
        fs,
        pool,
        definitor)
    {
    }
    
    
}