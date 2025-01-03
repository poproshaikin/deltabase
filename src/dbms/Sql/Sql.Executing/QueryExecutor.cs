using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Models;
using Data.Operation;
using Data.Operation.IO;
using Exceptions;
using Sql.Shared.Expressions;
using Sql.Shared.Queries;
using Utils;

namespace Sql.Executing;

public class QueryExecutor
{
    private string _dbName;
    
    private FileSystemHelper _fs;
    
    private DataServiceProvider _provider;
    
    public QueryExecutor(string dbName, FileSystemHelper fs)
    {
        _dbName = dbName;
        _fs = fs;
        _provider = new DataServiceProvider(dbName, fs, FileAccess.Read);
    }

    /// <summary>
    /// Executes a given SQL command based on its type.
    /// </summary>
    /// <param name="validatedQuery">The validated SQL query to be executed.</param>
    /// <returns>The <see cref="ExecutionResult"/> of the execution</returns>
    public IExecutionResult Execute(ISqlQuery validatedQuery)
    {
        try
        {
            return validatedQuery switch
            {
                SelectQuery select => ExecuteSelect(select),
                InsertQuery insert => ExecuteInsert(insert),

                _ => throw new NotImplementedException()
            };
        }
        catch (DbEngineException ex)
        {
            return new ExecutionResult(ex.Error);
        }
    }

    private ExecutionResult<PageRow[]> ExecuteSelect(SelectQuery query)
    {
        DataScanner scanner = _provider.CreateScanner();
        DataDefinitor definitor = _provider.CreateDefinitor();
        
        TableScheme tableScheme = definitor.GetTableScheme(query.From.TableName);
        TableModel read = scanner.Scan(tableScheme, query.Select.ColumnNames, query.Limit?.Limit, query.Condition);
        
        return new ExecutionResult<PageRow[]>(
            rowsAffected: read.Rows.Length,
            result: read.Rows);
    }

    private IExecutionResult ExecuteInsert(InsertQuery query)
    {
        DataInserter inserter = _provider.CreateInserter();
        DataDefinitor definitor = _provider.CreateDefinitor();
        DataSorter sorter = _provider.CreateSorter();
        
        string tableName = query.Insert.TableName;
        TableScheme scheme = definitor.GetTableScheme(tableName);

        int rowsAffected;
        
        _provider.SetStreamAccess(FileAccess.ReadWrite, tableName);

        foreach (ValuesExpr insertingDataSet in query.Values)
        {
            string[] finalDataSet = sorter.NeedsSort(scheme,
                query.Insert.ColumnNames)
                ? sorter.Sort(scheme,
                    passedColumns: query.Insert.ColumnNames,
                    passedValues: insertingDataSet)
                : insertingDataSet;

            inserter.Insert(scheme, finalDataSet);
        }

        _provider.SetStreamAccess(FileAccess.Read, tableName);
    }
}