using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Encoding;
using Data.Models;
using Data.Operation.IO;
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


    protected bool _encoding => _encoder is not null;
    
    protected IDataEncoder? _encoder;
    
    protected internal DataManipulator(string dbName,
        FileSystemHelper fs,
        FileStreamPool pool,
        DataDefinitor definitor,
        IDataEncoder? encoder)
    {
        _dbName = dbName;
        _fs = fs;
        _definitor = definitor;
        _pool = pool;
        _encoder = encoder;
    }
    
    protected async Task<PageHeader> ReadHeaderFromStart(FileStream stream)
    {
        int pageId = int.Parse((await ReadLineAndTryDecode(stream))!);
        
        int rowsCount = int.Parse((await ReadLineAndTryDecode(stream))!);
        
        string freeRowsLine = (await ReadLineAndTryDecode(stream))!;
        int[]? freeRows = freeRowsLine == "e" ? null : freeRowsLine.Split(',').Select(int.Parse).ToArray();
        
        return new PageHeader(pageId, rowsCount, freeRows);
    }

    protected RowModel ParseRow(string line)
    {
        string[] columns = line.Split(separator);
        
        return new RowModel(int.Parse(columns[0]), columns[1..]);
    }

    protected async Task SkipHeader(FileStream stream)
    {
        for (int i = 0; i < header_lines; i++)
        {
            _ = await ReadLineAndTryDecode(stream);
        }
    }

    protected Task<string?> ReadLineAndTryDecode(FileStream stream, bool turnOffEncoding = false)
    {
        string? line = ReadLine(stream);

        if (_encoding && !turnOffEncoding)
        {
            line = _encoder!.DecodeValue(line);
        }

        return Task.FromResult(line is "" ? null : line);
    }

    protected Task WriteLineAndTryEncode(FileStream stream, string line, bool turnOffEncoding = false)
    {
        if (_encoding && !turnOffEncoding)
        {
            line = _encoder!.EncodeValue(line);
        }
        
        return Task.CompletedTask;
    }

    private string ReadLine(FileStream stream)
    {
        List<byte> byteList = []; 
        // bytes are used because the return type of the stream.ReadByte() method is int, with -1 representing the end-of-stream marker.

        string newLineMarker = _encoding ? _encoder!.DecodeValue(new_line_marker.ToString()) : new_line_marker.ToString();
        
        int current;
        while ((current = stream.ReadByte()) != new_line_marker && current != -1)
        {
            byteList.Add(Convert.ToByte(current));
        }

        return ConvertHelper.GetString(byteList);
    }

    private void WriteLine(FileStream stream, string line)
    {
        string newLineMarker = _encoding ? _encoder!.EncodeValue(new_line_marker) : new_line_marker.ToString();
        
        byte[] buffer = ConvertHelper.GetBytes(line + newLineMarker);
        stream.Write(buffer, 0, buffer.Length);
    }
}