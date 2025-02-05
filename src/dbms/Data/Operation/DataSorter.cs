using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Operation.IO;
using Exceptions;
using Utils;

namespace Data.Operation;

public class DataSorter : DataManipulator
{
    internal DataSorter(string dbName,
        FileSystemHelper fsHelper,
        FileStreamPool pool,
        DataDescriptor descriptor) : base(dbName,
        fsHelper,
        pool,
        descriptor)
    {
    }
    
    /// <summary>
    /// Checks whether sorting of values is required before insertion 
    /// based on the provided column names.
    /// </summary>
    /// <param name="scheme">The table scheme containing metadata about the table struct    ure.</param>
    /// <param name="passedColumns">An array of column names passed for the operation.</param>
    /// <returns>True if sorting is required, otherwise false.</returns>
    public bool NeedsSort(TableScheme scheme, string[] passedColumns)
    {
        int i;
        for (i = 0; i < scheme.Columns.Length; i++)
        {
            // if wasn't passed the primary key column
            if (i == 0 &&
                scheme.PrimaryKey is not null && // if primary key column exists and
                scheme.Columns.Length == passedColumns.Length - 1 && // the scheme has one more column than the passed columns and
                scheme.Columns[1].Name == passedColumns[0]) // the name of the second schema column matches the first passed column
            {
                i++;
            }
            
            if (scheme.Columns[i].Name != passedColumns[i])
            {
                return true;
            }
        }

        return false;
    }

    public string[] Sort(TableScheme scheme, string[] passedColumns, string[] passedValues)
    {
        Dictionary<string, int> columnIndexMap = scheme.Columns
            .Select((columnScheme, index) => new { columnScheme.Name, Index = index })
            .ToDictionary(x => x.Name, x => x.Index);

        int[] selectedColumnsIds = passedColumns
            .Select(d => columnIndexMap.TryGetValue(d, out int index) 
                ? index 
                : throw new DbEngineException($"Column '{d}' not found in the scheme."))
            .ToArray();
        
        string?[] filteredAndSortedValues = selectedColumnsIds
            .Select(id => passedValues[id])
            .ToArray();

        return filteredAndSortedValues;
    }
}