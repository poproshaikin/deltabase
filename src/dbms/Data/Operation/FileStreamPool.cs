using System.Collections.Concurrent;
using Exceptions;

namespace Data.Operation;

internal class FileStreamPool : IDisposable
{
    internal ConcurrentDictionary<FileInfo, FileStream> Pool { get; set; }
    
    private FileAccess _fileAccess;
    
    internal FileStreamPool(FileAccess access)
    {
        Pool = new ConcurrentDictionary<FileInfo, FileStream>();
        _fileAccess = access;
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

    internal void ChangeAccess(FileAccess access, FileInfo file)
    {
        if (!Pool.ContainsKey(file))
        {
            throw new DbEngineException("The streams pool doesn't contain a passed file to access changing.");
        }

        long position = Pool[file].Position;
        
        Pool[file] = new FileStream(file.FullName, FileMode.Open, access);
        Pool[file].Position = position;
    }
    
    public void Dispose()
    {
        foreach (var kvp in Pool)
        {
            kvp.Value.Dispose();
        }
    }
}