using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Models;
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
    }

    private TableScheme? _cachedScheme;
    
    public void Insert(TableScheme scheme, string?[] finalDataSet)
    {
        Task.Run(InsertAsync(scheme, finalDataSet).Wait);
    }

    public async Task InsertAsync(TableScheme scheme, string?[] finalDataSet) 
    {
        // найти свободное место на страницах или создать новую
        // если свободное место: seek, обновление количества колонок и каждого значения 
        // если новая: запись новой строки

        _cachedScheme = scheme;
        
        FileInfo[] pages = _definitor.GetTableFiles(scheme.TableName, FileType.Record);
        
        // 1: insert to empty row
        // 2: insert to add of file
        // 3: create new file

        var context = await PrepareToRowInsertionAsync(pages);
    }

    private async Task<RowInsertionContext?> PrepareToRowInsertionAsync(FileInfo[] pages)
    {
        foreach (FileInfo page in pages)
        {
            BinaryDataIO io = new BinaryDataIO(_pool.GetOrOpen(page));
            PageHeader header = await io.ReadHeaderAsync();

            if (header.FreeRows.Length != 0)
            {
                int rowId = header.FreeRows[0];
                await io.SeekToRowAsync(rowId, _cachedScheme!);
            }
        }
    }

    // private async Task<RowInsertionContext?> TryFindPageWithEmptyRowsAsync(FileInfo[] pages)
    // {
    //     foreach (FileInfo page in pages)
    //     {
    //         FileStream pageStream = _pool.GetOrOpen(page);
    //         BinaryDataIO io = new BinaryDataIO(pageStream);
    //         
    //         PageHeader header = await io.ReadHeaderAsync();
    //         if (header.FreeRows.Length > 0)
    //         {
    //             return new RowInsertionContext(header, pageStream, RowInsertionOption.Inserting);
    //         }
    //     }
    //
    //     return null;
    // }
}

file readonly struct RowInsertionContext
{
    internal PageHeader Header { get; init; }
    
    internal FileStream Stream { get; init; }
    
    internal RowInsertionOption Option { get; init; }
    
    internal int NewRowId { get; init; }

    internal RowInsertionContext(PageHeader header, FileStream stream, RowInsertionOption option, int newRowId)
    {
        Header = header;
        Stream = stream;
        Option = option;
        NewRowId = newRowId;
    }
}

file enum RowInsertionOption
{
    Inserting,
    Appending,
    CreatingNewPage,
}