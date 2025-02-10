using System.Text;
using Data.Definitions.Schemes;
using Data.Models;
using Enums.Records.Columns;

namespace Data.Operation;

internal abstract class PageReader : TableFileAccessor
{
    protected abstract Stream Stream { get; }
    
    protected PageReader(DataPageManager manager) : base(manager)
    {
    }

    protected async Task<byte[]> ReadBytesAsync(int count)
    {
        byte[] bytes = new byte[count];
        _ = await Stream.ReadAsync(bytes, 0, count);
        return bytes;
    }
    
    public async Task<byte> ReadByteAsync()
    {
        byte[] bytes = await ReadBytesAsync(1);
        return bytes[0];
    }
    
    internal async Task<int> ReadIntAsync()
    {
        byte[] bytes = await ReadBytesAsync(sizeof(int));
        return BitConverter.ToInt32(bytes, 0);
    }

    internal async Task<float> ReadFloatAsync()
    {
        byte[] bytes = await ReadBytesAsync(sizeof(float));
        return BitConverter.ToSingle(bytes, 0);
    }

    internal async Task<char> ReadCharAsync()
    {
        byte[] bytes = await ReadBytesAsync(sizeof(char));
        return BitConverter.ToChar(bytes, 0);
    }

    internal async Task<bool> ReadBoolAsync()
    {
        byte[] bytes = await ReadBytesAsync(sizeof(bool));
        return BitConverter.ToBoolean(bytes, 0);
    }

    internal async Task<string> ReadStringAsync()
    {
        int length = await ReadIntAsync();
        byte[] bytes = await ReadBytesAsync(length);
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
        int[] freeRows = await ReadArrayAsync(ReadIntAsync);
        
        return new PageHeader(pageId, rowsCount, freeRows);
    }

    internal async Task<PageRow> ReadRowAsync(TableScheme scheme)
    {
        _ = await ReadIntAsync(); // row length, not needed
        int rid = await ReadIntAsync();

        bool[] nullBitmap = await ReadNullBitmapAsync(scheme);

        var values = new string?[scheme.Columns.Length];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = await ReadRowValueAsync(scheme.Columns[i].ValueType, nullBitmap[i]);
        }

        return new PageRow(rid, values);
    }

    internal async Task<T[]> ReadArrayAsync<T>(Func<Task<T>> itemReader)
    {
        int count = await ReadIntAsync();
        return await ReadArrayAsync(itemReader, count);
    }

    internal async Task<T[]> ReadArrayAsync<T>(Func<Task<T>> itemReader, int count)
    {
        if (count == 0) return [];
        if (count == 1) return [await itemReader()];
        
        T[] result = new T[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = await itemReader();
        }
        
        return result;
    }

    private async Task<string?> ReadRowValueAsync(SqlValueType valueType, bool isNull) =>
        isNull
            ? null 
            : (await ReadAsTypeAsync(valueType)).ToString();

    private async Task<bool[]> ReadNullBitmapAsync(TableScheme scheme)
    {
        // TODO можно битмапу сделать собственно под биты 
        // считать по количеству колонок количество байтов на битмапу
        // использовать битовую арифметику
        var bitmap = new bool[scheme.Columns.Length];
        for (int i = 0; i < bitmap.Length; i++)
        {
            bitmap[i] = await ReadByteAsync() is 1;
        }
        return bitmap;
    }
}