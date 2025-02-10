using System.Text;
using Data.Definitions.Schemes;
using Data.Models;
using Enums.Records.Columns;

namespace Data.Operation;

internal abstract class PageWriter : TableFileAccessor
{
    protected abstract Stream Stream { get; }
    
    protected PageWriter(DataPageManager manager) : base(manager)
    {
    }

    protected async Task WriteBytesAsync(byte[] bytes)
    {
        await Stream.WriteAsync(bytes);
    }

    internal async Task WriteByteAsync(byte value)
    {
        await WriteBytesAsync([value]);
    }
    
    internal async Task WriteIntAsync(int value)
    {
        await WriteBytesAsync(BitConverter.GetBytes(value));
    }
    
    internal async Task WriteIntAsync(uint value)
    {
        await WriteBytesAsync(BitConverter.GetBytes(value));    
    }

    internal async Task WriteFloatAsync(float value)
    {
        await WriteBytesAsync(BitConverter.GetBytes(value));
    }

    internal async Task WriteCharAsync(char value)
    {
        await WriteBytesAsync(BitConverter.GetBytes(value));
    }

    internal async Task WriteBoolAsync(bool value)
    {
        await WriteBytesAsync(BitConverter.GetBytes(value));
    }

    internal async Task WriteStringAsync(string value)
    {
        int length = value.Length;
        await WriteIntAsync(length);
        await WriteBytesAsync(Encoding.UTF8.GetBytes(value));
    }

    internal async Task WriteAsTypeAsync(object value, SqlValueType valueType)
    {
        ArgumentNullException.ThrowIfNull(value);

        string valueStr = value.ToString()!;

        if (valueType == SqlValueType.Integer)
            await WriteIntAsync(int.Parse(valueStr));

        else if (valueType == SqlValueType.String)
            await WriteStringAsync(valueStr);

        else if (valueType == SqlValueType.Char)
            await WriteCharAsync(valueStr[0]);

        else if (valueType == SqlValueType.Float)
            await WriteFloatAsync(float.Parse(valueStr));

        else if (valueType == SqlValueType.Boolean)
            await WriteBoolAsync(bool.Parse(valueStr.ToLower()));

        throw new NotImplementedException();
    }

    internal async Task WriteHeaderAsync(PageHeader pageHeader)
    {
        await WriteIntAsync(pageHeader.PageId);
        await WriteIntAsync(pageHeader.RowsCount);
        await WriteArrayAsync(pageHeader.FreeRows.AsObjectCollection(), SqlValueType.Integer);
    }

    internal async Task WriteRowAsync(PageRow pageRow, TableScheme scheme)
    {
        int byteLength = 0; // need to prepend it in start of a row
        
        await WriteIntAsync(pageRow.RId);
        byteLength += sizeof(int);

        await WriteNullBitmapAsync(pageRow.Data);

        for (int i = 0; i < pageRow.ValuesCount; i++)
        {
            await WriteRowValueAsync(pageRow[i], scheme.Columns[i].ValueType);
        }
    }

    internal async Task WriteArrayAsync(object collection, SqlValueType valueType)
    {
        ArgumentNullException.ThrowIfNull(collection);
        
        if (collection is object[] array) 
            await WriteArrayAsync(array, SqlValueType.Integer);
        
        if (collection is IEnumerable<object> enumerable) 
            await WriteArrayAsync(enumerable.ToArray(), SqlValueType.Integer);
        
        throw new InvalidOperationException("Tried to write an object of type " + collection.GetType().FullName + " as array");
    }
    
    internal async Task WriteArrayAsync(object[] array, SqlValueType valueType)
    {
        int count = array.Length;
        await WriteIntAsync(count);

        if (count == 0) return;
        if (count == 1) await WriteAsTypeAsync(array[0], valueType);

        for (int i = 0; i < count; i++)
        {
            await WriteAsTypeAsync(array[i], valueType);
        }
    }

    private async Task WriteRowValueAsync(string? value, SqlValueType valueType)
    {
        if (value is not null) 
            await WriteAsTypeAsync(value, valueType);
    }
    
    private async Task WriteNullBitmapAsync(string?[] dataSet)
    {
        foreach (string? value in dataSet)
        {
            await WriteByteAsync(value is null ? (byte)1 : (byte)0);
        }
    }
}

file static class Extensions
{
    public static object[] AsObjectCollection<T>(this T[] array)
    {
        return array.Select<T, object>(o => o).ToArray();
    }
}