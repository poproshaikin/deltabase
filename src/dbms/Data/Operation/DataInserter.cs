using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Operation.IO;
using Enums.FileSystem;
using Utils;

namespace Data.Operation;

public class DataInserter : DataManipulator
{
    internal DataInserter(string dbName,
        FileSystemHelper fs,
        FileStreamPool pool,
        DataDefinitor definitor) : base(dbName,
        fs,
        pool,
        definitor)
    {
        _cachedHeaders = [];
    }

    private List<PageHeader> _cachedHeaders;

    public void Insert(TableScheme scheme, string[] finalDataSet)
    {
        Task.Run(InsertAsync(scheme, finalDataSet).Wait);
    }

    public async Task InsertAsync(TableScheme scheme, string[] finalDataSet) 
    {
        // найти свободное место на страницах или создать новую
        // если свободное место: seek, обновление количества колонок и каждого значения 
        // если новая: запись новой строки

        FileInfo[] pages = _definitor.GetTableFiles(scheme.TableName, FileType.Record);
        var pageWithEmptyRows = await TryFindPageWithEmptyRowsAsync(pages);

        if (pageWithEmptyRows is { } page)
        {
            BinaryDataIO io = new BinaryDataIO(page.Stream, scheme);
            int emptyRowId = page.Header.FreeRows[0];
            
            // header is already read in the TryFindPageWithEmptyRows method
            await io.SeekToRowAsync(emptyRowId);
        }
    }

    private async Task<(PageHeader Header, FileStream Stream)?> TryFindPageWithEmptyRowsAsync(FileInfo[] pages)
    {
        foreach (FileInfo page in pages)
        {
            FileStream pageStream = _pool.GetOrOpen(page);
            BinaryDataIO io = new BinaryDataIO(pageStream);
            
            PageHeader header = await io.ReadHeaderAsync();
            if (header.FreeRows.Length > 0)
            {
                return (header, pageStream);
            }
        }

        return null;
    }
}