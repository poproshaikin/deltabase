using System.Text;
using Enums.Exceptions;
using Utils;
using Utils.Settings;

namespace Sql.Shared.Execution;

public class ExecutionResult
{
    public int? RowsAffected { get; private set; }
    public long? ExecutionTime { get; set; }
    public string[]? DataSet { get; private set; }
    public ErrorType? Error { get; set; }
    
    public ExecutionResult(int rowsAffected, IEnumerable<string>? dataSet)
    {
        RowsAffected = rowsAffected;
        DataSet = dataSet?.ToArray();
    }

    public ExecutionResult(int rowsAffected, long executionTime, IEnumerable<string>? dataSet) : this(rowsAffected,
        dataSet)
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
               (DataSet is null ? "" : SerializeDataSet());
    }

    public static ExecutionResult Deserialize(string formatted)
    {
        if (formatted[0] == 'e')
            return new ExecutionResult((ErrorType)int.Parse(formatted.Split(':')[1]));
        
        string[] formattedRows = formatted.Split('\n');

        int rowsAffected = int.Parse(formattedRows[0].Split(':')[1]);
        long executionTime = long.Parse(formattedRows[1].Split(':')[1]);
        string[]? dataSet = formattedRows.Length > 2 ? formattedRows[2].Split('\r', StringSplitOptions.RemoveEmptyEntries) : null;
        
        return new ExecutionResult(rowsAffected, executionTime, dataSet);
    }

    private string SerializeDataSet()
    {
        return string.Join("\r\n", DataSet!);
    }

    public ResponseSettings ToResponseSettings()
    {
        ResponseSettings responseSettings = new ResponseSettings();
        
        if (RowsAffected is not null)
            responseSettings.Add("rows_affected", RowsAffected.Value);
        
        if (ExecutionTime is not null)
            responseSettings.Add("execution_time", ExecutionTime.Value);
        
        if (Error is not null)
            responseSettings.Add("error", Error.Value);
        
        if (DataSet is not null)
            responseSettings.Add("data", SerializeDataSet());
        
        return responseSettings;
    }
}