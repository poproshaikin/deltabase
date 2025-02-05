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
        DataDescriptor descriptor) : base(dbName,
        fs,
        pool,
        descriptor)
    {
    }

    private TableScheme? _cachedScheme;
    
    public RowInsertionResult Insert(TableScheme scheme, string[] finalDataSet)
    {
        return Task.Run(() =>
        {
            var task = InsertAsync(scheme, finalDataSet);
            task.Wait();
            return task.Result;
        }).Result;
    }

    public async Task<RowInsertionResult> InsertAsync(TableScheme scheme, string[] finalDataSet) 
    {
        _cachedScheme = scheme;
        
        FileInfo[] pages = Descriptor.GetTableFiles(scheme.TableName, FileType.Record);
        
        // 1: insert to an empty row
        // 2: insert to an end of file
        // 3: create new file 

        var formatter = new BinaryDataFormatter();
        ulong size = formatter.EstimateSize(scheme, finalDataSet);
        
        var context = await PrepareToRowInsertionAsync(pages, size);
        if (context is null)
            return RowInsertionResult.FailedToSelectInsertionFile;
        
        await ProcessInsertionAsync(context, );
    }

    private async Task<RowInsertionContext?> PrepareToRowInsertionAsync(FileInfo[] pages, ulong size)
    {
        foreach (FileInfo page in pages)
        {
            FileStream stream = _pool.GetOrOpen(page);
            BinaryDataIO io = new(stream);
            PageHeader header = await io.ReadHeaderAsync();

            if (header.FreeRows.Length != 0)
            {
                int rowId = header.FreeRows[0];
                await io.SeekToRowAsync(rowId, _cachedScheme!, true);
                return new RowInsertionContext(header, page, stream, RowInsertionOption.Inserting, rowId);
            }

            if (header.Size + size <= MAXIMUM_PAGE_SIZE)
            {
                int rowsCount = header.RowsCount;
                io.SeekToEnd();
                return new RowInsertionContext(header, page, stream, RowInsertionOption.Appending, newRowId: rowsCount);
            }
            
            ...
        }
    }

    private async Task ProcessInsertionAsync(RowInsertionContext context, string[] data)
    {
        
    }
}

internal readonly struct RowInsertionContext
{
    internal PageHeader Header { get; init; }
    
    internal FileInfo File { get; init; }
    
    internal FileStream Stream { get; init; }
    
    internal RowInsertionOption Option { get; init; }
    
    internal int NewRowId { get; init; }
    
    internal RowInsertionContext(PageHeader header, FileInfo file, FileStream stream, RowInsertionOption option, int newRowId)
    {
        Header = header;
        File = file;
        Stream = stream;
        Option = option;
        NewRowId = newRowId;
    }
}

public enum RowInsertionResult
{
    Success,
    FailedToSelectInsertionFile, 
}

internal enum RowInsertionOption
{
    Inserting,
    Appending,
    CreatingNewPage,
}