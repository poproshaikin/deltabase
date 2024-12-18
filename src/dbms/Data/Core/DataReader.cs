using System.Data;
using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Models;
using Sql.Shared.Expressions;
using Utils;

namespace Data.Core;

public class DataReader : DataManipulator, IDisposable
{
    private string _dbName;
    private FileSystemHelper _fs;
    private DataDefinitor _dataDefinitor;
    private FileStreamsPool _fileStreamsPool;

    public DataReader(string dbName, FileSystemHelper fs)
    {
        _dbName = dbName;
        _fs = fs;
        _dataDefinitor = new DataDefinitor(dbName, fs);
        _fileStreamsPool = new FileStreamsPool(FileAccess.Read);
    }

    public TableModel Read(TableScheme tableScheme,
        string[]? passedColumns,
        int? rowsLimit,
        ConditionGroup? conditionGroup)
    {
        return Task.Run(() => ReadAsync(tableScheme, passedColumns, rowsLimit, conditionGroup).GetAwaiter().GetResult()).Result;
    }
    
    public async Task<TableModel> ReadAsync(TableScheme tableScheme,
        string[]? passedColumns,
        int? rowsLimit,
        ConditionGroup? conditionGroup)
    {
        IEnumerable<PageReadingPlan> readingPlans = await GetReadingPlans(tableScheme, passedColumns, rowsLimit, conditionGroup);
        IEnumerable<Task<RowModel[]>> readingTasks = readingPlans.Select(plan => ReadPageAsync(tableScheme, plan));
        RowModel[] sorted = await SortManyPagesData(readingTasks);
        return new TableModel(tableScheme, sorted);
    }

    private async Task<PageReadingPlan[]> GetReadingPlans(TableScheme tableScheme,
        string[]? passedColumns,
        int? rowsLimit,
        ConditionGroup? conditionGroup)
    {
        string tablePath = _fs.GetRecordFolderPath(_dbName, tableScheme.TableName);
        
        DirectoryInfo tableDir = new(tablePath);
        FileInfo[] allPages = tableDir.GetFiles("*.record")
            .OrderBy(f => int.Parse(f.Name.Split('.')[1])) // get the number of file and sort
            .ToArray();
        
        if (rowsLimit is null)
        {
            return allPages.Select(page => new PageReadingPlan(page, null, passedColumns, conditionGroup, false)).ToArray();
        }
        else
        {
            return await GetReadingPlansWithinLimitAsync(allPages, rowsLimit.Value, passedColumns, conditionGroup);
        }
    }

    private async Task<PageReadingPlan[]> GetReadingPlansWithinLimitAsync(FileInfo[] allPages,
        int rowsLimit,
        string[]? passedColumns,
        ConditionGroup? conditionGroup)
    {
        List<PageReadingPlan> readingPlans = [];
        
        int rowsToReadCount = rowsLimit;

        foreach (FileInfo page in allPages)
        {
            StreamReader streamReader = _fileStreamsPool.GetOrOpen(page);
            PageHeader header = await ReadHeaderFromStart(streamReader);

            if (header.RowsCount <= rowsToReadCount)
            {
                readingPlans.Add(new PageReadingPlan(page, null, passedColumns, conditionGroup, true));
            }
            else
            {
                readingPlans.Add(new PageReadingPlan(page, rowsToReadCount, passedColumns, conditionGroup, true));
            }
            
            rowsToReadCount -= header.RowsCount;

            if (rowsToReadCount <= 0)
            {
                break;
            }
        }
            
        return readingPlans.ToArray();
    }

    private async Task<RowModel[]> ReadPageAsync(TableScheme tableScheme, PageReadingPlan plan)
    {
        StreamReader streamReader = _fileStreamsPool.GetOrOpen(plan.PageToRead);
        
        List<RowModel> listToLoad = [];
        ConditionChecker checker = new(tableScheme, plan.ConditionGroup!);
        
        if (plan.IsHeaderRead is false)     
        {
            await SkipHeader(streamReader);
            plan.IsHeaderRead = true;
        }
        
        while ((await streamReader.ReadLineAsync()) is { } line)
        {
            RowModel readRow = ParseRow(line);
            
            if (plan.PassedColumns is not null)
            {
                readRow.Data = FilterAndSort(tableScheme, plan.PassedColumns, readRow);
            }
            
            if (plan.RowsToReadCount is not null && listToLoad.Count >= plan.RowsToReadCount)
            {
                break;
            }
            
            if (plan.ConditionGroup is not null)
            {
                if (!checker.Check(readRow))
                {
                    continue;
                }
            }
            
            listToLoad.Add(readRow);
        }
        
        return listToLoad.ToArray();
    }

    private string[] FilterAndSort(TableScheme tableScheme, string[] passedColumns, RowModel readRow)
    {
        int[] selectedColumnsIds = passedColumns.Select(d => Array.IndexOf(tableScheme.Columns.Select(c => c.Name).ToArray(), d)).ToArray();  // gets an id of each selected column
        string[] filteredAndSortedValues = new string[selectedColumnsIds.Length];

        for (int i = 0; i < selectedColumnsIds.Length; i++)
        {
            filteredAndSortedValues[i] = readRow.Data[selectedColumnsIds[i]];
        }

        return filteredAndSortedValues;
    }

    private async Task<RowModel[]> SortManyPagesData(IEnumerable<Task<RowModel[]>> readingTasks)
    {
        List<RowModel> sorted = [];

        foreach (Task<RowModel[]> readingTask in readingTasks)
        {
            sorted.AddRange(await readingTask);
        }

        return sorted.ToArray();
    }
    
    public TableModel SelectScalar(TableModel read)
    {
        return new TableModel(read.Scheme, read.Rows.FirstOrDefault() );
    }

    public void Dispose()
    {
        _fileStreamsPool.Dispose();
    }
}