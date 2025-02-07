using System.Text;
using Enums.Records.Columns;

namespace Data.Operation;

internal abstract class TableFileWriter : TableFileAccessor
{
    private protected Stream Stream { get; private set; }

    private protected abstract Task WriteBytesAsync(byte[] bytes);

    private protected TableFileWriter(Stream stream)
    {
        Stream = stream;
    }

    internal async Task WriteIntAsync(int value)
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

    internal async Task WriteAsTypeAsync(SqlValueType valueType, object value)
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
}