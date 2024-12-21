namespace Data.Definitions.Schemes;

internal struct PageHeader
{
    internal int PageId { get; set; }
    internal int RowsCount { get; set; }
    internal int[]? FreeRows { get; set; }

    internal PageHeader(int pageId, int rowsCount, int[]? freeRows)
    {
        PageId = pageId;
        RowsCount = rowsCount;
        FreeRows = freeRows;
    }
}