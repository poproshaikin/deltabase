using Data.Definitions.Schemes;
using Data.Models;
using Data.Operation.IO;
using Enums.FileSystem;
using Utils;

namespace Data.Operation;

public class DataInserter : DataManipulator
{
    internal DataInserter(string dbName,
        FileSystemHelper fsHelper,
        FileStreamPool pool,
        DataDescriptor descriptor) : base(dbName,
        fsHelper,
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
        
        RowInsertionContext? context = await PrepareInsertionAsync(pages, size);
        if (!context.HasValue)
            return RowInsertionResult.FailedToSelectInsertionFile;
        
        return await ProcessInsertionAsync(context.Value, finalDataSet);
    }

    private async Task<RowInsertionContext?> PrepareInsertionAsync(FileInfo[] pages, ulong size)
    {
        foreach (var page in pages)
        {
            FileStream stream = Pool.GetOrOpen(page);
            BinaryDataIO io = new(stream);
            PageHeader header = await io.ReadHeaderAsync();
            
            // // if page hasn't enough place for new row
            if (header.Size + size > MAXIMUM_PAGE_SIZE) 
                continue;
            
            if (header.FreeRows.Length == 0)
            {
                int rowsCount = header.RowsCount;
                io.SeekToEnd();
                return new RowInsertionContext(header, page, stream, RowInsertionOption.Appending, newRowId: rowsCount);
            }
            else
            {
                int rowId = header.FreeRows[0];
                await io.SeekToRowAsync(rowId, _cachedScheme!, true);
                return new RowInsertionContext(header, page, stream, RowInsertionOption.Inserting, rowId);
            }
        }
        
        // создать класс по типу PageHandler,
        // через который будут проходить операции ввода/вывода
        // и заодно управление страницей
    }

    private async Task<RowInsertionResult> ProcessInsertionAsync(RowInsertionContext context, string[] data)
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