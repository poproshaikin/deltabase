using Utils;

namespace Db.Records;

public class RecordsWriter
{
    private FileSystemManager _fs;
    private string _dbName;
    
    public RecordsWriter(FileSystemManager fs, string dbName)
    {
        _fs = fs;
        _dbName = dbName;
    }

    public void Write(Record record, RecordRow newRow)
    {
        if (record.HasPkColumn)
        {
            record.AddRowAndOrder(newRow);
        }
        else
        {
            record.AddRow(newRow);
        }
        
        Write(record);
    }
    
    public void Write(Record record) => WriteAsync(record).Wait();

    public async Task WriteAsync(Record record)
    {
        List<string> rowsList = [..record.Rows.Select(r => r.ToString())];
        rowsList = rowsList.Prepend("[").ToList();
        rowsList.Add("]");
        await _fs.WriteRecordAsync(_dbName, record.Name, rowsList);
    }
}