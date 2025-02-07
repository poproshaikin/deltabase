using Data.Definitions;
using Utils;

namespace Data.Operation;

public class DataCreater : DataManipulator
{
    internal DataCreater(string dbName,
        FileSystemHelper fsHelper,
        FileStreamPool pool,
        DataDescriptor descriptor) : base(dbName,
        fsHelper,
        pool,
        descriptor)
    {
    }
    
    
}