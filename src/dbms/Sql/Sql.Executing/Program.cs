using System.Diagnostics;
using Data.Definitions;
using Enums;
using Enums.Sql;
using Sql.Shared.Queries;
using Utils;

namespace Sql.Executing;

class Program
{
    private static string _rawQuery;
    private static string _serverName;
    private static string _dbName;
    
    static void Main(string[] args)
    {
        SetInput(args);

        Stopwatch stopwatch = Stopwatch.StartNew();
        
        FileSystemHelper fs = new FileSystemHelper(_serverName);
        
        QueryParser parser = new QueryParser();
        QueryValidator validator = new QueryValidator(_dbName, fs);
        QueryExecutor executor = new QueryExecutor(_dbName, fs);
            
        ISqlQuery parsedQuery = parser.Parse(_rawQuery);
        ValidationResult validationResult = validator.Validate(parsedQuery);
        IExecutionResult executionResult;
        
        if (!validationResult.IsValid)
        {
            executionResult = new ExecutionResult(validationResult.Error);
        }
        else
        {
            executionResult = executor.Execute(parsedQuery);
            executionResult.ExecutionTime = stopwatch.ElapsedMilliseconds;
        }

        Console.Out.WriteLine(executionResult.Serialize());
    }

    static void SetInput(string[] args)
    {
        bool showHelp = args is ["-h"];
        
        if (showHelp) Console.Write("Raw query: ");
        _rawQuery = Console.In.ReadLine()!;
        if (showHelp) Console.Write("Server name: ");
        _serverName = Console.In.ReadLine()!;
        if (showHelp) Console.Write("Database name: ");
        _dbName = Console.In.ReadLine()!;
    }
}