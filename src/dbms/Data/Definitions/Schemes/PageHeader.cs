namespace Data.Definitions.Schemes;

public struct PageHeader
{
    public int PageId { get; set; }
    public int RowsCount { get; set; }
    public int[] FreeRows { get; set; }
    
    public PageHeader(int pageId, int rowsCount, int[] freeRows)
    {
        PageId = pageId;
        RowsCount = rowsCount;
        FreeRows = freeRows;
    }
}