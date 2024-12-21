using Data.Definitions;
using Data.Encoding;
using Utils;

namespace Data.Operation.IO;

public class DataWriter : DataManipulator
{
    internal DataWriter(string dbName,
        FileSystemHelper fs,
        FileStreamPool pool,
        DataDefinitor definitor,
        IDataEncoder? encoder) : base(dbName,
        fs,
        pool,
        definitor,
        encoder)
    {
    }
}