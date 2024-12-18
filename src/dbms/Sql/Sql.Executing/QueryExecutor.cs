using Data.Core;
using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Models;
using Enums.Sql;
using Exceptions;
using Sql.Shared.Execution;
using Sql.Shared.Queries;
using Utils;

namespace Sql.Executing.App;

public class QueryExecutor
{
    private string _dbName;
    private FileSystemHelper _fs;
    
    public QueryExecutor(string dbName, FileSystemHelper fs)
    {
        _dbName = dbName;
        _fs = fs;
    }
    
    /// <summary>
    /// Executes a given SQL command based on its type.
    /// </summary>
    /// <param name="validatedQuery">The validated SQL query to be executed.</param>
    /// <returns>The <see cref="ExecutionResult"/> of the execution</returns>
    public ExecutionResult Execute(ISqlQuery validatedQuery)
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

    private ExecutionResult ExecuteSelect(SelectQuery query)
    {
        using DataReader reader = new DataReader(_dbName, _fs);
        DataDefinitor definitor = new DataDefinitor(_dbName, _fs);
        
        TableScheme tableScheme = definitor.GetTableScheme(query.From.TableName);
        TableModel read = reader.Read(tableScheme, query.Select.ColumnNames, query.Limit?.Limit, query.Condition);
        
        return new ExecutionResult(
            rowsAffected: read.Rows.Length,
            dataSet: read.Rows.Select(r => r.ToString()));
    }

    private ExecutionResult ExecuteInsert(InsertQuery query)
    {
        throw new NotImplementedException();
    }
}