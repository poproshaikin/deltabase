using Data.Definitions.Schemes;

namespace Data.Operation;

public class TableFileMetadata
{
    public string FileName => FileInfo.Name;
    
    public ulong FileSize => (ulong)FileInfo.Length;
    
    public FileInfo FileInfo { get; set; }
    
    public TableScheme Scheme { get; set; }
    
    
    public TableFileMetadata(FileInfo fileInfo, TableScheme scheme)
    {
        FileInfo = fileInfo;
        Scheme = scheme;
    }
}