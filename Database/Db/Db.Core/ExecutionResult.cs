using Db.DataStorage;

namespace Db.Core;

public class ExecutionResult
{
    public int RowsAffected { get; private set; }
    public long ExecutionTime { get; private set; }
    public TableDataSet? DataSet { get; private set; }
    
    public ExecutionResult(TableDataSet dataSet, int rowsAffected, long executionTime)
    {
        DataSet = dataSet;
        RowsAffected = rowsAffected;
        ExecutionTime = executionTime;
    }

    public byte[] Format()
    {
        throw new NotImplementedException();
    }
}