using Data.Definitions;
using Data.Operation.IO;
using Utils;

namespace Data.Operation;

public class DataSorter : DataManipulator
{
    internal DataSorter(string dbName,
        FileSystemHelper fs,
        FileStreamPool pool,
        DataDefinitor definitor) : base(dbName,
        fs,
        pool,
        definitor)
    {
    }
    
    
}