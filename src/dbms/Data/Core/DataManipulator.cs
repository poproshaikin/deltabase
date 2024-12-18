using System.Data;
using Data.Definitions.Schemes;
using Data.Models;

namespace Data.Core;

public abstract class DataManipulator
{
    protected const int header_lines = 3;
    protected const string separator = "■ ";
    
    protected virtual async Task<PageHeader> ReadHeaderFromStart(StreamReader streamReader)
    {
        int pageId = int.Parse((await streamReader.ReadLineAsync())!);
        
        int rowsCount = int.Parse((await streamReader.ReadLineAsync())!);
        
        string freeRowsLine = (await streamReader.ReadLineAsync())!;
        int[]? freeRows = freeRowsLine == "e" ? null : freeRowsLine.Split(',').Select(int.Parse).ToArray();
        
        return new PageHeader(pageId, rowsCount, freeRows);
    }

    protected virtual RowModel ParseRow(string line)
    {
        string[] columns = line.Split(separator);
        
        return new RowModel(int.Parse(columns[0]), columns[1..]);
    }

    protected virtual async Task SkipHeader(StreamReader streamReader)
    {
        for (int i = 0; i < header_lines; i++)
        {
            _ = await streamReader.ReadLineAsync();
        }
    }
}