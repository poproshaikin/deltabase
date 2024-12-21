using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Encoding;
using Data.Models;
using Data.Operation.IO;
using Exceptions;
using Utils;

namespace Data.Operation;

public abstract class DataManipulator
{
    protected const int header_lines = 3;
    
    protected const string separator = "■ ";

    protected const char new_line_marker = '\n';
    

    protected string _dbName;
    
    protected FileSystemHelper _fs;
    
    protected DataDefinitor _definitor;
    
    protected FileStreamPool _pool;
    
    private protected DataManipulator(string dbName,
        FileSystemHelper fs,
        FileStreamPool pool,
        DataDefinitor definitor)
    {
        _dbName = dbName;
        _fs = fs;
        _definitor = definitor;
        _pool = pool;
    }
    
    private protected async Task<PageHeader> ReadHeader(FileStream stream)
    {
        int pageId = await ReadIntAsync(stream);
        int rowsCount = await ReadIntAsync(stream);
        int[] freeRows = await ReadFreeRowsAsync(stream);
        
        return new PageHeader(pageId, rowsCount, freeRows);
    }

    protected async Task SkipHeader(FileStream stream)
    {
        for (int i = 0; i < header_lines; i++)
        {
            await SkipRowAsync(stream);
        }
    }

#region Binary IO
    
    protected async Task<PageRow> ReadRowAsync(FileStream stream)
    {
        List<byte[]> values = [];
        int valuesCount = await ReadIntAsync(stream);
        int rid = await ReadIntAsync(stream);
        
        for (int i = 0; i < valuesCount - 1; i++)
        {
            values.Add(await ReadBlockAsync(stream));
        }
        string[] columnValues = new string[valuesCount - 1];
        for (int i = 0; i < columnValues.Length; i++)
        {
            columnValues[i] = ConvertHelper.GetString(values[i + 1]);
        }

        return new PageRow(rid, columnValues);
    }

    protected async Task WriteRowAsync(FileStream stream, PageRow row)
    {
        int valuesCount = row.ValuesCount;
        await WriteIntAsync(stream, valuesCount);
        await WriteIntAsync(stream, row.RId);
        
        for (int i = 0; i < valuesCount; i++)
        {
            byte[] blockData = ConvertHelper.GetBytes(row[i]);
            await WriteBlockAsync(stream, blockData);
        }
    }

    private async Task<int[]> ReadFreeRowsAsync(FileStream stream)
    {
        int freeRowsCount = await ReadIntAsync(stream);
        int[] freeRows = new int[freeRowsCount];
        
        for (int i = 0; i < freeRowsCount; i++)
        {
            freeRows[i] = await ReadIntAsync(stream);
        }

        return freeRows;
    }

    private async Task WriteFreeRowsAsync(FileStream stream, int[] freeRows)
    {
        int freeRowsCount = freeRows.Length;
        await WriteIntAsync(stream, freeRowsCount);

        for (int i = 0; i < freeRowsCount; i++)
        {
            byte[] pointerBuffer = BitConverter.GetBytes(freeRows[i]);
            await stream.WriteAsync(pointerBuffer, 0, sizeof(short));
        }
    }
    
    private async Task<int> ReadIntAsync(FileStream stream)
    {
        byte[] buffer = new byte[sizeof(int)];
        int read = await stream.ReadAsync(buffer, 0, sizeof(int));

        if (read != sizeof(int))
            throw new NotImplementedException();
            
        return BitConverter.ToInt32(buffer, 0);
    }
    
    private async Task WriteIntAsync(FileStream stream, int value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        
        await stream.WriteAsync(buffer, 0, sizeof(int));
    }

    private async Task WriteStringAsync(FileStream stream, string value)
    {
        byte[] buffer = ConvertHelper.GetBytes(value);
        await WriteBlockAsync(stream, buffer);
    }
    
    private async Task<byte[]> ReadBlockAsync(FileStream stream)
    {
        int valueLength = await ReadIntAsync(stream);
        byte[] buffer = new byte[valueLength];

        _ = stream.ReadAsync(buffer, 0, valueLength);

        return buffer;
    }
    
    private async Task WriteBlockAsync(FileStream stream, byte[] data)
    {
        int valueLength = data.Length;
        byte[] lengthBytes = BitConverter.GetBytes(valueLength);
        data = lengthBytes.Concat(data).ToArray();
        
        await stream.WriteAsync(data, 0, data.Length);
    }

    private async Task SkipRowAsync(FileStream stream)
    {
        int blocksCount = await ReadIntAsync(stream);

        for (int i = 0; i < blocksCount; i++)
        {
            await SkipBlockAsync(stream);
        }
    }

    private async Task SkipBlockAsync(FileStream stream)
    {
        int blockLength = await ReadIntAsync(stream);
        stream.Seek(blockLength, SeekOrigin.Current);
    }
    
#endregion
}