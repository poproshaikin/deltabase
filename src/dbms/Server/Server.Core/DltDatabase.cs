using System.Diagnostics;
using Sql.Executing;
using Workers;

namespace Server.Core;

internal class DltDatabase
{
    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public string Name { get; private set; }

    private string _serverName;

    public DltDatabase(string name, string serverName)
    {
        Name = name;
        _serverName = serverName;
    }

    public IExecutionResult ExecuteRequest(string rawQuery)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        SqlWorker sqlWorker = new(); 
        
        IExecutionResult executionResult = sqlWorker.DoParseAndExecute(rawQuery, _serverName, Name)!;
        
        executionResult.ExecutionTime = stopwatch.ElapsedMilliseconds;
        return executionResult;
    }
}