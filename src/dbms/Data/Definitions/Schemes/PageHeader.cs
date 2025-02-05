namespace Data.Definitions.Schemes;

public struct PageHeader
{
    public int PageId { get; set; }
    public int RowsCount { get; set; }
    public int[] FreeRows { get; set; }
    
    internal FileInfo? File { get; set; }

    public ulong? Size => (ulong)File?.Length!;

    internal PageHeader(int pageId, int rowsCount, int[] freeRows)
    {
        PageId = pageId;
        RowsCount = rowsCount;
        FreeRows = freeRows;
        File = null;
    }

    internal PageHeader(int pageId, int rowsCount, int[] freeRows, FileInfo file) : this(pageId, rowsCount, freeRows)
    {
        File = file;
    }
}