using System.Text;
using Data.Definitions.Schemes;
using Data.Models;
using Enums.Records.Columns;
using Utils;

namespace Data.Operation.IO;

public class BinaryDataIO
{
    private const int header_lines = 3;

    private const string separator = "■ ";

    private const char new_line_marker = '\n';
    
    private readonly FileStream _stream;
    
    public BinaryDataIO(FileStream stream)
    {
        _stream = stream;
    }

    public async Task<PageHeader> ReadHeaderAsync()
    {
        int pageId = await ReadIntAsync();
        int rowsCount = await ReadIntAsync();
        int[] freeRows = await ReadFreeRowsAsync();
        
        return new PageHeader(pageId, rowsCount, freeRows);
    }

    public async Task WriteHeader(PageHeader header)
    {
        int pageId = header.PageId;
        int rowsCount = header.RowsCount;
        int[] freeRows = header.FreeRows;

        await WriteIntAsync(pageId);
        await WriteIntAsync(rowsCount);
        await WriteFreeRowsAsync(freeRows);
    }

    public async Task SkipHeaderAsync()
    {
        for (int i = 0; i < header_lines; i++)
        {
            await SkipRowAsync();
        }
    }
    
    public async Task<PageRow> ReadRowAsync(TableScheme scheme)
    {
        int valuesCount = await ReadIntAsync();
        int rid = await ReadIntAsync();
        
        object[] values = new object[valuesCount];
        for (int i = 0; i < valuesCount; i++)
        {
            values[i] = scheme.Columns[i].ValueType switch
            {
                SqlValueType.Integer => await ReadIntAsync(),
                SqlValueType.String => await ReadStringAsync(),
                SqlValueType.Char => await ReadCharAsync(),
                SqlValueType.Float => await ReadFloatAsync(),
                
                //TODO доделать остальные типы
                _ => throw new NotImplementedException(),
            };
        }

        return new PageRow(rid, values.ToArray());
    }
    
    public async Task WriteRowAsync(PageRow row, TableScheme scheme)
    {
        int valuesCount = row.ValuesCount;
        await WriteIntAsync(valuesCount);
        await WriteIntAsync(row.RId);
        
        for (int i = 0; i < valuesCount; i++)
        {
            switch (scheme.Columns[i].ValueType)
            {
                case SqlValueType.Integer: await WriteIntAsync((int)row[i]); break;
                case SqlValueType.String: await WriteStringAsync((string)row[i]); break;
                case SqlValueType.Char: await WriteCharAsync((char)row[i]); break;
                case SqlValueType.Float: await WriteFloatAsync((float)row[i]); break;
                
                // TODO доделать остальные типы
                default: throw new NotImplementedException(); break;
            }
        }
    }
    
    public async Task SkipRowAsync()
    {
        int blocksCount = await ReadIntAsync();

        for (int i = 0; i < blocksCount; i++)
        {
            await SkipBlockAsync();
        }
    }

    public async Task<int[]> ReadFreeRowsAsync()
    {
        int freeRowsCount = await ReadIntAsync();
        int[] freeRows = new int[freeRowsCount];
        
        for (int i = 0; i < freeRowsCount; i++)
        {
            freeRows[i] = await ReadIntAsync();
        }

        return freeRows;
    }

    public async Task WriteFreeRowsAsync(int[] freeRows)
    {
        int freeRowsCount = freeRows.Length;
        await WriteIntAsync(freeRowsCount);

        for (int i = 0; i < freeRowsCount; i++)
        {
            byte[] pointerBuffer = BitConverter.GetBytes(freeRows[i]);
            await _stream.WriteAsync(pointerBuffer, 0, sizeof(short));
            await _stream.FlushAsync();
        }
    }
    
    public async Task<int> ReadIntAsync()
    {
        byte[] buffer = new byte[sizeof(int)];
        int read = await _stream.ReadAsync(buffer, 0, sizeof(int));

        if (read != sizeof(int))
            throw new NotImplementedException();
            
        return BitConverter.ToInt32(buffer, 0);
    }
    
    public async Task WriteIntAsync(int value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        
        await _stream.WriteAsync(buffer, 0, sizeof(int));
        await _stream.FlushAsync();
    }

    public async Task<float> ReadFloatAsync()
    {
        byte[] buffer = new byte[sizeof(float)];
        _ = await _stream.ReadAsync(buffer, 0, sizeof(float));
        return BitConverter.ToSingle(buffer);
    }

    public async Task WriteFloatAsync(float value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        
        await _stream.WriteAsync(buffer, 0, sizeof(float));
        await _stream.FlushAsync();
    }
    
    public async Task<string> ReadStringAsync()
    {
        int length = await ReadIntAsync();
        byte[] buffer = new byte[length];
        _ = await _stream.ReadAsync(buffer, 0, length);
        return ConvertHelper.GetString(buffer);
    }
    
    public async Task WriteStringAsync(string value)
    {
        byte[] buffer = ConvertHelper.GetBytes(value);
        await WriteBlockAsync(buffer);
    }

    public async Task<char> ReadCharAsync()
    {
        byte[] buffer = await ReadBlockAsync();
        return Encoding.UTF8.GetChars(buffer)[0];
    }

    public async Task WriteCharAsync(char value)
    {
        byte[] buffer = Encoding.UTF8.GetBytes([value]);
        await WriteBlockAsync(buffer);
    }
    
    public async Task<byte[]> ReadBlockAsync()
    {
        int valueLength = await ReadIntAsync();
        byte[] buffer = new byte[valueLength];

        _ = _stream.ReadAsync(buffer, 0, valueLength);

        return buffer;
    }
    
    public async Task WriteBlockAsync(byte[] data)
    {
        int valueLength = data.Length;
        byte[] lengthBytes = BitConverter.GetBytes(valueLength);
        data = lengthBytes.Concat(data).ToArray();
        
        await _stream.WriteAsync(data, 0, data.Length);
        await _stream.FlushAsync();
    }

    public async Task<byte> ReadByteAsync()
    {
        byte[] buffer = new byte[sizeof(byte)];
        _ = await _stream.ReadAsync(buffer, 0, sizeof(byte));
        return buffer[0];
    }

    public async Task WriteByteAsync(byte value)
    {
        await _stream.WriteAsync([value], 0, 1);
    }

    public async Task SkipBlockAsync()
    {
        int blockLength = await ReadIntAsync();
        _stream.Seek(blockLength, SeekOrigin.Current);
    }

    public async Task WriteRowStartAsync(int columnsCount)
    {
        byte[] countBytes = BitConverter.GetBytes(columnsCount);
        
        await _stream.WriteAsync(countBytes, 0, sizeof(int));
        await _stream.FlushAsync();
    }  

    public byte[] GetBytes(object value)
    {
        if (value is int @int)
        {
            return BitConverter.GetBytes(@int);
        }

        if (value is float @float)
        {
            return BitConverter.GetBytes(@float);
        }

        if (value is double @double)
        {
            return BitConverter.GetBytes(@double);
        }

        if (value is char @char)
        {
            return BitConverter.GetBytes(@char);
        }

        if (value is bool @bool)
        {
            return BitConverter.GetBytes(@bool);
        }

        if (value is string @string)
        {
            return System.Text.Encoding.UTF8.GetBytes(@string);
        }

        if (value is int[] intArray)
        {
            return intArray.Aggregate<int, byte[]>([], (sum, current) => sum.Concat(BitConverter.GetBytes(current)).ToArray());
        }

        if (value is float[] floatArray)
        {
            return floatArray.Aggregate<float, byte[]>([], (sum, current) => sum.Concat(BitConverter.GetBytes(current)).ToArray());
        }

        if (value is double[] doubleArray)
        {
            return doubleArray.Aggregate<double, byte[]>([], (sum, current) => sum.Concat(BitConverter.GetBytes(current)).ToArray());
        }
        
        if (value is string[] stringArray)
        {
            return stringArray.Aggregate<string, byte[]>([], (sum, current) => sum.Concat(System.Text.Encoding.UTF8.GetBytes(current)).ToArray());
        }

        throw new NotImplementedException();
    }
}