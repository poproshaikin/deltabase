using System.Collections.Concurrent;

namespace Data.Core;

internal class FileStreamsPool : IDisposable
{
    private ConcurrentDictionary<FileInfo, StreamReader> _pool;
    private FileAccess _fileAccess;
    
    internal FileStreamsPool(FileAccess access)
    {
        _pool = new ConcurrentDictionary<FileInfo, StreamReader>();
        _fileAccess = access;
    }
    
    internal StreamReader GetOrOpen(FileInfo file)
    {
        if (_pool.TryGetValue(file, out StreamReader? reader))
        {
            return reader!;
        }
        else
        {
            FileStream newStream = new FileStream(file.FullName, FileMode.Open, _fileAccess);
            _pool[file] = new StreamReader(newStream);
            return _pool[file];
        }
    }
    
    public void Dispose()
    {
        foreach (var kvp in _pool)
        {
            kvp.Value.Dispose();
        }
    }
}