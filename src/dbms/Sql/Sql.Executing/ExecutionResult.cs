using System.Data;
using Data.Models;
using Enums.Exceptions;
using Utils.Settings;

namespace Sql.Executing;

public interface IExecutionResult
{
    int? RowsAffected { get; set; }
    
    long? ExecutionTime { get; set; }
    
    ErrorType? Error { get; set; }

    string Serialize();

    static IExecutionResult Deserialize(string formatted)
    {
        if (formatted[0] == 'e')
            return new ExecutionResult((ErrorType)int.Parse(formatted.Split(':')[1]));
        
        string[] formattedRows = formatted.Split('\n');
    
        int rowsAffected = int.Parse(formattedRows[0].Split(':')[1]);
        long executionTime = long.Parse(formattedRows[1].Split(':')[1]);
        string[]? dataSet = formattedRows.Length > 2 ? formattedRows[2].Split('\r', StringSplitOptions.RemoveEmptyEntries) : null;
        
        return new ExecutionResult<string[]>(rowsAffected, executionTime, dataSet);
    }
}

internal class ExecutionResult<T> : IExecutionResult
{
    public int? RowsAffected { get; set; }
    
    public long? ExecutionTime { get; set; }
    
    public T? Result { get; set; }
    
    public ErrorType? Error { get; set; }
    
    public ExecutionResult(int rowsAffected, T? result)
    {
        RowsAffected = rowsAffected;
        Result = result;
    }

    public ExecutionResult(int rowsAffected, long executionTime, T? result) : this(rowsAffected,
        result)
    {
        ExecutionTime = executionTime;
    }

    public ExecutionResult(ErrorType error)
    {
        Error = error;
    }

    public string Serialize()
    {
        return $"rowsAffected:{RowsAffected}\n" +
               $"executionTime:{ExecutionTime}\n" + 
               (Result is null ? "" : SerializeDataSet());
    }

    // public static ExecutionResult Deserialize(string formatted)
    // {
    //     if (formatted[0] == 'e')
    //         return new ExecutionResult((ErrorType)int.Parse(formatted.Split(':')[1]));
    //     
    //     string[] formattedRows = formatted.Split('\n');
    //
    //     int rowsAffected = int.Parse(formattedRows[0].Split(':')[1]);
    //     long executionTime = long.Parse(formattedRows[1].Split(':')[1]);
    //     string[]? dataSet = formattedRows.Length > 2 ? formattedRows[2].Split('\r', StringSplitOptions.RemoveEmptyEntries) : null;
    //     
    //     return new ExecutionResult(rowsAffected, executionTime, dataSet);
    // }

    private string SerializeDataSet()
    {
        if (Result is IEnumerable<string> stringArr)
        {
            return SerializeStringCollection(stringArr);
        }

        if (Result is IEnumerable<object> objectArr)
        {
            return SerializeStringCollection(objectArr.Select(o => o?.ToString()));
        }

        if (Result is TableModel tableModel)
        {
            return SerializeRowsCollection(tableModel.Rows);
        }

        throw new NotImplementedException();

        string SerializeStringCollection(IEnumerable<string?> collection)
        {
            return string.Join("\r\n", collection);
        }

        string SerializeRowsCollection(IEnumerable<PageRow> collection)
        {
            return SerializeStringCollection(collection.Select(p => p.ToString()));
        }
    }
}

internal class ExecutionResult : ExecutionResult<object>
{
    public ExecutionResult(int rowsAffected, object result) : base(rowsAffected, result)
    {
    }

    public ExecutionResult(int rowsAffected, long executionTime, object result) : base(rowsAffected, executionTime, result)
    {
    }

    public ExecutionResult(ErrorType error) : base(error)
    {
    }
}