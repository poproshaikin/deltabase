using System.Collections;
using Data.Definitions.Schemes;
using Data.Models;

namespace Data.Operation;

internal class PageModel : IEnumerable<PageRow>
{
    // сделать индексаторы, когда меняется значение на индексаторе - поменять значение в файле
    // для этого PageRow должна быть readonly struct

    private readonly DataPageManager _pageManager;

    private PageReader? _cachedReader;
    private PageWriter? _cachedWriter;
    
    
    public string FileName => _fileInfo.Name;
    
    public ulong FileSize => (ulong)_fileInfo.Length;

    public string Path => _fileInfo.FullName;
    

    private readonly FileInfo _fileInfo;
    
    private readonly TableScheme _scheme;


    private List<PageRow> _cachedRows;


    public PageModel(FileInfo fileInfo, TableScheme scheme, DataPageManager pageManager)
    {
        _fileInfo = fileInfo;
        _scheme = scheme;
        _pageManager = pageManager;
        _cachedRows = [];
    }

    private void GetRow(int rowId)
    {
        
    }
    
    public IEnumerator<PageRow> GetEnumerator()
    {
        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}