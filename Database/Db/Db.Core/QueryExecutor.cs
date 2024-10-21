using System.Diagnostics;
using Db.DataStorage;
using Sql.Queries;
using Sql.Tokens;

namespace Db.Core;

public class QueryExecutor
{
    private RecordsManager _recordsManager;
    
    public QueryExecutor(RecordsManager manager)
    {
        _recordsManager = manager;
    }
    
    /// <summary>
    /// Executes a given SQL command based on its type.
    /// </summary>
    /// <param name="query">The validated SQL query to be executed.</param>
    /// <returns>The <see cref="ExecutionResult"/> of the execution</returns>
    public ExecutionResult Execute(IValidatedQuery query)
    {
        return query switch
        {
            SelectQuery select => ExecuteSelect(select),

            _ => throw new NotImplementedException()
        };
    }

    private ExecutionResult ExecuteSelect(SelectQuery query)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        TableSchema targetTable = _recordsManager.GetTableSchema(query.From.TableName);
        
        TableDataSet finalDataSet = _recordsManager.GetTableData(targetTable, 
            columnNames: query.Select.ColumnNames, 
            condition: query.Condition,
            rowsLimit: query.Limit?.Limit ?? int.MaxValue);
        
        return new ExecutionResult(finalDataSet,
            rowsAffected: finalDataSet.RowsCount,
            executionTime: stopwatch.ElapsedMilliseconds);
    }

    // 
    // /// <exception cref="NotImplementedException">Thrown if the command type is not implemented or recognized.</exception>
    // private byte[] ExecuteQuery(SqlQuery command)
    // {
    //     try
    //     {
    //         if (_validator.IsInvalid(command, out ResponseType error))
    //         {
    //             return ConvertHelper.GetBytes(error);
    //         }
    //         
    //         return command switch
    //         {
    //             SelectQuery select => ExecuteReader(select),
    //             InsertQuery insert => ExecuteInsert(insert),
    //             UpdateQuery update => ExecuteUpdate(update),
    //             DeleteQuery delete => ExecuteDelete(delete),
    //             CreateTableQuery create => ExecuteCreate(create),
    //
    //             _ => throw new NotImplementedException()
    //         };
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine(ex);
    //         return ConvertHelper.GetBytes(ResponseType.InternalServerError);
    //     }
    // }
}