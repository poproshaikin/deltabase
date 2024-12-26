using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Models;
using Data.Operation.IO;
using Sql.Shared.Expressions;
using Utils;

namespace Data.Operation;

public class DataScanner : DataManipulator
{
    internal DataScanner(string dbName,
        FileSystemHelper fs,
        FileStreamPool pool,
        DataDefinitor definitor) : base(dbName,
        fs,
        pool,
        definitor)
    {
    }

    private TableScheme? _cachedScheme;

    public TableModel Read(TableScheme tableScheme,
        string[]? passedColumns,
        int? rowsLimit,
        ConditionGroup? conditionGroup)
    {
        return Task.Run(() => ReadAsync(tableScheme, passedColumns, rowsLimit, conditionGroup).GetAwaiter().GetResult())
            .Result;
    }

    public async Task<TableModel> ReadAsync(TableScheme tableScheme,
        string[]? passedColumns,
        int? rowsLimit,
        ConditionGroup? conditionGroup)
    {
        _cachedScheme = tableScheme;
        
        var readingPlans = await GetReadingPlans(tableScheme, passedColumns, rowsLimit, conditionGroup);
        var readingTasks = readingPlans.Select(plan => ReadPageAsync(tableScheme, plan));
        var sorted = await SortManyPagesData(readingTasks);
        
        return new TableModel(tableScheme, sorted);
    }

    private async Task<PageReadingPlan[]> GetReadingPlans(TableScheme tableScheme,
        string[]? passedColumns,
        int? rowsLimit,
        ConditionGroup? conditionGroup)
    {
        string tablePath = _fs.GetRecordFolderPath(_dbName, tableScheme.TableName);

        DirectoryInfo tableDir = new(tablePath);
        var allPages = tableDir.GetFiles("*.record")
            .OrderBy(f => int.Parse(f.Name.Split('.')[1])) // get the number of file and sort
            .ToArray();

        if (rowsLimit is null)
            return allPages.Select(page => new PageReadingPlan(page, _cachedScheme, null, passedColumns, conditionGroup, false))
                .ToArray();
        return await GetReadingPlansWithinLimitAsync(allPages, rowsLimit.Value, passedColumns, conditionGroup);
    }

    private async Task<PageReadingPlan[]> GetReadingPlansWithinLimitAsync(FileInfo[] allPages,
        int rowsLimit,
        string[]? passedColumns,
        ConditionGroup? conditionGroup)
    {
        List<PageReadingPlan> readingPlans = [];
        
        int rowsToReadCount = rowsLimit;

        foreach (FileInfo pageInfo in allPages)
        {
            FileStream stream = _pool.GetOrOpen(pageInfo);
            BinaryDataIO io = new(stream);
            PageHeader header = await io.ReadHeaderAsync();

            readingPlans.Add(
                new PageReadingPlan(pageInfo,
                    _cachedScheme!,
                    rowsToReadCount: header.RowsCount <= rowsToReadCount
                        ? null
                        : rowsToReadCount,
                    passedColumns,
                    conditionGroup,
                    isHeaderRead: true));

            rowsToReadCount -= header.RowsCount;

            if (rowsToReadCount <= 0) break;
        }

        return readingPlans.ToArray();
    }

    private async Task<PageRow[]> ReadPageAsync(TableScheme tableScheme, PageReadingPlan plan)
    {
        FileStream stream = _pool.GetOrOpen(plan.PageToRead);
        BinaryDataIO io = new(stream);

        List<PageRow> listToLoad = [];
        ConditionChecker checker = new(tableScheme, plan.ConditionGroup!);

        if (plan.IsHeaderRead is false)
        {
            await io.SkipHeaderAsync();
            plan.IsHeaderRead = true;
        }

        while (await io.ReadRowAsync(_cachedScheme!) is { } readPageRow)
        {
            if (plan.PassedColumns is not null) readPageRow.Data = FilterAndSort(tableScheme, plan.PassedColumns, readPageRow);

            if (plan.RowsToReadCount is not null && listToLoad.Count >= plan.RowsToReadCount) break;

            if (plan.ConditionGroup is not null)
                if (!checker.Check(readPageRow))
                    continue;

            listToLoad.Add(readPageRow);
        }

        return listToLoad.ToArray();
    }

    private object[] FilterAndSort(TableScheme tableScheme, string[] passedColumns, PageRow readPageRow)
    {
        var selectedColumnsIds = passedColumns
            .Select(d => Array.IndexOf(tableScheme.Columns.Select(c => c.Name).ToArray(), d))
            .ToArray(); // gets an id of each selected column
        var filteredAndSortedValues = new object[selectedColumnsIds.Length];

        for (var i = 0; i < selectedColumnsIds.Length; i++)
            filteredAndSortedValues[i] = readPageRow.Data[selectedColumnsIds[i]];

        return filteredAndSortedValues;
    }

    private async Task<PageRow[]> SortManyPagesData(IEnumerable<Task<PageRow[]>> readingTasks)
    {
        List<PageRow> sorted = [];

        foreach (var readingTask in readingTasks) sorted.AddRange(await readingTask);

        return sorted.ToArray();
    }
}