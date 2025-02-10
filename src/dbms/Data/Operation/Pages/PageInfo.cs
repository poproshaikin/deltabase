using Data.Definitions.Schemes;

namespace Data.Operation;

public class PageInfo
{
    public string FileName => FileInfo.Name;
    
    public ulong FileSize => (ulong)FileInfo.Length;

    public string Path => FileInfo.FullName;
    
    public FileInfo FileInfo { get; set; }
    
    public TableScheme Scheme { get; set; }


    public PageInfo(FileInfo fileInfo, TableScheme scheme)
    {
        FileInfo = fileInfo;
        Scheme = scheme;
    }
}