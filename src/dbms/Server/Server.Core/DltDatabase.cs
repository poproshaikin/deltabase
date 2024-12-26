using System.Diagnostics;
using Sql.Shared.Execution;
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

    public ExecutionResult ExecuteRequest(string rawQuery)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        SqlWorker sqlWorker = new(); 
        
        ExecutionResult executionResult = sqlWorker.DoParseAndExecute(rawQuery, _serverName, Name)!;
        
        executionResult.ExecutionTime = stopwatch.ElapsedMilliseconds;
        return executionResult;
    }
}