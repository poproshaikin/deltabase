using System.Collections.Concurrent;
using Exceptions;

namespace Data.Operation.IO;

public class FileStreamPool : IDisposable
{
    internal ConcurrentDictionary<FileInfo, FileStream> Pool;
    private FileAccess _fileAccess;
    
    public FileStreamPool(FileAccess access)
    {
        Pool = new ConcurrentDictionary<FileInfo, FileStream>();
        _fileAccess = access;
    }

    public void ChangeAccess(FileAccess access, FileInfo file)
    {
        if (!Pool.ContainsKey(file))
        {
            throw new DbEngineException("The streams pool doesn't contain a passed file to access changing.");
        }
        
        Pool[file] = new FileStream(file.FullName, FileMode.Open, access);
    }
    
    internal FileStream GetOrOpen(FileInfo file)
    {
        if (Pool.TryGetValue(file, out FileStream? stream))
        {
            return stream;
        }
        else
        {
            return 
                Pool[file] = new FileStream(file.FullName, FileMode.Open, _fileAccess);
        }
    }
    
    public void Dispose()
    {
        foreach (var kvp in Pool)
        {
            kvp.Value.Dispose();
        }
    }
}