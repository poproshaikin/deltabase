using Utils;

namespace Db.Core;

public static class DltDatabaseInitializer
{
    public static DltDatabase Init(string name, FileSystemManager fs)
    {
        return new DltDatabase(name, fs);
    }
}