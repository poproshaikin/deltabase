using System.Text;
using Data.Definitions.Schemes;
using Data.Models;
using Enums.Exceptions;
using Enums.Records.Columns;
using Exceptions;
using Utils;

namespace Data.Operation.IO;

public class BinaryDataIO
{
    private const int header_lines = 3;

    private const string separator = "■ ";

    private const char new_line_marker = '\n';
    
    protected readonly FileStream _stream;

    protected readonly TableScheme? _scheme;
    
    protected readonly BinaryDataFormatter Formatter;

    public BinaryDataIO(FileStream stream)
    {
        _stream = stream;
        Formatter = new BinaryDataFormatter();
    }
    
    public BinaryDataIO(FileStream stream, TableScheme scheme) : this(stream)
    {
        _scheme = scheme;
    }

    public async Task<PageRow> ReadRowAsync(TableScheme scheme)
    {
        int rowLength = await ReadIntAsync();
        int rid = await ReadIntAsync();

        bool[] nullBitmap = ReadNullBitmap();

        var values = new string?[scheme.Columns.Length];
        for (int i = 0; i < values.Length; i++)
        {
            if (nullBitmap[i] is true)
            {
                values[i] = null;
                continue;
            }
            
            SqlValueType valueType = scheme.Columns[i].ValueType;

            object value;
            
            if (valueType == SqlValueType.Integer)
                value = await ReadIntAsync();

            else if (valueType == SqlValueType.String)
                value = await ReadStringAsync();

            else if (valueType == SqlValueType.Char)
                value = await ReadCharAsync();

            else if (valueType == SqlValueType.Float)
                value = await ReadFloatAsync();

            else if (valueType == SqlValueType.Boolean)
                value = await ReadBooleanAsync();

            else
                throw new NotImplementedException();

            values[i] = value.ToString();
        }

        return new PageRow(rid, values);

        bool[] ReadNullBitmap()
        {
            var bitmap = new bool[scheme.Columns.Length];
            for (int i = 0; i < bitmap.Length; i++)
            {
                bitmap[i] = ReadByte() is 1;
            }
            return bitmap;
        }
    }

    public async Task WriteRowAsync(PageRow row)
    {
        
    }
    
    public async Task InsertRowAsync(PageRow row)
    {
        throw new NotImplementedException();
    }
    
    public async Task<PageHeader> ReadHeaderAsync()
    {
        int pageId = await ReadIntAsync();
        int rowsCount = await ReadIntAsync();
        int[] freeRows = await ReadArrayAsync(ReadIntAsync);
        
        return new PageHeader(pageId, rowsCount, freeRows, new FileInfo(_stream.Name));
    }

    public async Task<T[]> ReadArrayAsync<T>(Func<Task<T>> readingFunction) where T : unmanaged
    {
        int elementsCount = await ReadIntAsync();
        
        if (elementsCount == 0) return [];
        if (elementsCount == 1) return [await readingFunction()];
        
        T[] result = new T[elementsCount];
        for (int i = 0; i < elementsCount; i++)
        {
            result[i] = await readingFunction();
        }
        
        return result;
    }

    public byte ReadByte()
    {
        return (byte)_stream.ReadByte();
    }
    
    public async Task<int> ReadIntAsync()
    {
        byte[] buffer = await ReadBytesAsync(sizeof(int));
        return Formatter.DeserializeAsInt(buffer);
    }

    public async Task<float> ReadFloatAsync()
    {
        byte[] buffer = await ReadBytesAsync(sizeof(float));
        return Formatter.DeserializeAsFloat(buffer);
    }
    
    public async Task<string> ReadStringAsync()
    {
        int length = await ReadIntAsync();
        byte[] buffer = await ReadBytesAsync(length);
        return Formatter.DeserializeAsString(buffer);
    }

    public async Task<char> ReadCharAsync()
    {
        int length = await ReadIntAsync();
        byte[] buffer = await ReadBytesAsync(length);
        return Formatter.DeserializeAsChar(buffer);
    }

    public async Task<bool> ReadBooleanAsync()
    {
        byte[] buffer = await ReadBytesAsync(sizeof(bool));
        return BitConverter.ToBoolean(buffer, 0);
    }
    
    public async Task<byte[]> ReadBytesAsync(int count)
    {
        byte[] result = new byte[count];
        await _stream.ReadExactlyAsync(result, 0, count);
        return result;
    }

    public void SkipBytes(int count)
    {
        _stream.Seek(count, SeekOrigin.Current);
    }

    public async Task SkipBlockAsync()
    {
        int blockLength = await ReadIntAsync();
        _stream.Seek(blockLength, SeekOrigin.Current);
    }

    public async Task SkipArrayAsync(int? elementSize)
    {
        int elementsCount = await ReadIntAsync();

        if (elementSize is not null)
        {
            for (int i = 0; i < elementsCount; i++)
            {
                SkipBytes(elementSize.Value);
            }
        }
        else
        {
            for (int i = 0; i < elementsCount; i++)
            {
                await SkipBlockAsync();
            }
        }
    }
    
    public async Task SkipHeaderAsync()
    {
        SkipBytes(sizeof(int));
        SkipBytes(sizeof(int));
        await SkipArrayAsync(sizeof(int));
    }

    public async Task SkipColumnAsync(int columnId)
    {
        await SkipColumnAsync(columnId, _scheme ?? throw new DbEngineException(ErrorType.InternalServerError));
    }

    public async Task SkipColumnAsync(int columnId, TableScheme scheme)
    {
        SqlValueType valueType = scheme.Columns[columnId].ValueType;
        
        if (valueType.IsFixedSize)
        {
            int length = valueType.Size!.Value;
            Seek(length);
        }
        else
        {
            await SkipBlockAsync();
        }
    }

    public void Seek(int count, SeekOrigin seekOrigin = SeekOrigin.Current)
    {
        _stream.Seek(count, seekOrigin);
    }
    
    public async Task SeekToRowAsync(int rowId, bool headerRead)
    {
        await SeekToRowAsync(rowId, _scheme ?? throw new DbEngineException(ErrorType.InternalServerError), headerRead);
    }

    public async Task SeekToRowAsync(int rowId, TableScheme scheme, bool headerRead)
    {
        if (!headerRead)
            await SkipHeaderAsync();
        
        int rid;
        do
        {
            rid = await ReadIntAsync();

            if (rid == rowId)
            {
                break;
            }

            ColumnScheme[] columns = scheme.Columns;

            for (int i = 0; i < columns.Length; i++)
            {
                await SkipColumnAsync(columnId: i, scheme);
            }
        } while (rid != rowId);
    }

    public void SeekToEnd()
    {
        _stream.Seek(0, SeekOrigin.End);
    }
}