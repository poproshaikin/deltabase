using Data.Definitions;
using Data.Encoding;
using Utils;

namespace Data.Operation.IO;

public class DataInserter : DataManipulator
{
    internal DataInserter(string dbName,
        FileSystemHelper fs,
        FileStreamPool pool,
        DataDefinitor definitor) : base(dbName,
        fs,
        pool,
        definitor)
    {
    }
}