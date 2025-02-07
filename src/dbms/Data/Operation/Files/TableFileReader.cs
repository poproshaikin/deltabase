using System.Text;
using Data.Definitions.Schemes;
using Data.Models;
using Enums.Records.Columns;

namespace Data.Operation;

internal abstract class TableFileReader : TableFileAccessor
{
    // предоставлять возможность чтения разными способами,
    // например FileStream и класс File
    // определять при создании сущности

    private protected Stream Stream { get; private set; }
    
    private protected abstract Task<byte[]> ReadBytes(int length);

    
    private protected TableFileReader(Stream stream)
    {
        Stream = stream;
    }

    private async Task<byte> ReadByteAsync()
    {
        byte[] bytes = await ReadBytes(1);
        return bytes[0];
    }
    
    internal async Task<int> ReadIntAsync()
    {
        byte[] bytes = await ReadBytes(sizeof(int));
        return BitConverter.ToInt32(bytes, 0);
    }

    internal async Task<float> ReadFloatAsync()
    {
        byte[] bytes = await ReadBytes(sizeof(float));
        return BitConverter.ToSingle(bytes, 0);
    }

    internal async Task<char> ReadCharAsync()
    {
        byte[] bytes = await ReadBytes(sizeof(char));
        return BitConverter.ToChar(bytes, 0);
    }

    internal async Task<bool> ReadBoolAsync()
    {
        byte[] bytes = await ReadBytes(sizeof(bool));
        return BitConverter.ToBoolean(bytes, 0);
    }

    internal async Task<string> ReadStringAsync()
    {
        int length = await ReadIntAsync();
        byte[] bytes = await ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    internal async Task<object> ReadAsTypeAsync(SqlValueType valueType)
    {
        if (valueType == SqlValueType.Integer)
            return await ReadIntAsync();

        else if (valueType == SqlValueType.String)
            return await ReadStringAsync();

        else if (valueType == SqlValueType.Char)
            return await ReadCharAsync();

        else if (valueType == SqlValueType.Float)
            return await ReadFloatAsync();
        
        else if (valueType == SqlValueType.Boolean)
            return await ReadBoolAsync();
        
        throw new NotImplementedException();
    }

    internal async Task<PageHeader> ReadHeaderAsync()
    {
        int pageId = await ReadIntAsync();
        int rowsCount = await ReadIntAsync();
        int[] freeRows = await ReadArray(ReadIntAsync);
        
        return new PageHeader(pageId, rowsCount, freeRows);
    }

    internal async Task<PageRow> ReadRowAsync(TableScheme scheme)
    {
        _ = await ReadIntAsync(); // row length, not needed
        int rid = await ReadIntAsync();

        bool[] nullBitmap = await ReadNullBitmap(scheme);

        var values = new string?[scheme.Columns.Length];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = await ReadRowValueAsync(scheme.Columns[i].ValueType, nullBitmap[i]);
        }

        return new PageRow(rid, values);
    }

    internal async Task<T[]> ReadArray<T>(Func<Task<T>> itemReader)
    {
        int length = await ReadIntAsync();

        if (length == 0) return [];
        if (length == 1) return [await itemReader()];
        
        T[] result = new T[length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = await itemReader();
        }
        
        return result;
    }

    private async Task<string?> ReadRowValueAsync(SqlValueType valueType, bool isNull) =>
        isNull
            ? null
            : (await ReadAsTypeAsync(valueType)).ToString();

    private async Task<bool[]> ReadNullBitmap(TableScheme scheme)
    {
        // TODO можно битмапу сделать собственно под биты 
        // считать по количеству колонок количество байтов на битмапу
        // использовать 
        var bitmap = new bool[scheme.Columns.Length];
        for (int i = 0; i < bitmap.Length; i++)
        {
            bitmap[i] = await ReadByteAsync() is 1;
        }
        return bitmap;
    }
}