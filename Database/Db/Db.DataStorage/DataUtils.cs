using Exceptions;

namespace Db.DataStorage;

internal static class DataUtils
{
    public static RowDataSet[] ToRows(IReadOnlyList<ColumnDataSet> columns)
    {
        List<RowDataSet> rows = [];
            
        for (int i = 0; i < CountRows(columns); i++)
        {
            string[] values = columns.Select(c => c[i]).ToArray();
            rows.Add(new RowDataSet(values));
        }

        return rows.ToArray();
    }

    public static ColumnDataSet[] ToColumns(IReadOnlyList<RowDataSet> rows)
    {
        List<ColumnDataSet> columns = [];
        
        for (int i = 0; i < CountColumns(rows); i++)
        {
            string[] values = rows.Select(c => c[i]).ToArray();
            columns.Add(new ColumnDataSet(values));
        }

        return columns.ToArray();
    }

    public static int CountColumns(IReadOnlyList<RowDataSet> rows)
    {
        if (rows.Count == 0)
            return 0;

        int firstCount = rows.First().Count;

        if (rows.Any(c => c.Count != firstCount))
            return -1;

        return firstCount;
    }

    public static int CountRows(IReadOnlyList<ColumnDataSet> columns)
    {
        if (columns.Count == 0)
            return 0;

        int firstCount = columns.First().RowsCount;

        if (columns.Any(c => c.RowsCount != firstCount))
            return -1;

        return firstCount;
    }
}