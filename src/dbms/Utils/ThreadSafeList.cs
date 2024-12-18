using System.Collections;

namespace Utils;

public class ThreadSafeList<T> : IEnumerable<T>
{
    private readonly List<T> _list = [];
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    public void Add(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            _list.Add(item);
        }
        finally
        {
            _lock.ExitWriteLock(); 
        }
    }

    public bool Remove(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _list.Remove(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public List<T> GetAll()
    {
        _lock.EnterReadLock();
        try
        {
            return new List<T>(_list);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool Contains(T item)
    {
        _lock.EnterReadLock();  
        try
        {
            return _list.Contains(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _list.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();  
        try
        {
            _list.Clear();
        }
        finally
        {
            _lock.ExitWriteLock(); 
        }
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            foreach (var item in _list)
            {
                yield return item;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}