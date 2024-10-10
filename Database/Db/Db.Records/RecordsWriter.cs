using Enums.FileSystem;
using Utils;

namespace Db.Records;

/// <summary>
/// Provides methods to write records and their rows to a database.
/// </summary>
public class RecordsWriter
{
    private FileSystemManager _fs;
    private string _dbName;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordsWriter"/> class.
    /// </summary>
    /// <param name="fs">The <see cref="FileSystemManager"/> used for file operations.</param>
    /// <param name="dbName">The name of the database to which records will be written.</param>
    public RecordsWriter(FileSystemManager fs, string dbName)
    {
        _fs = fs;
        _dbName = dbName;
    }

    /// <summary>
    /// Writes a new row to the specified record. If the record has a primary key column, the new row is added in order.
    /// </summary>
    /// <param name="record">The <see cref="Record"/> to which the new row will be added.</param>
    /// <param name="newRow">The <see cref="RecordRow"/> to be added to the record.</param>
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
    
    /// <summary>
    /// Writes the entire record to the database synchronously.
    /// </summary>
    /// <param name="record">The <see cref="Record"/> to write to the database.</param>
    public void Write(Record record) => WriteAsync(record).Wait();

    /// <summary>
    /// Asynchronously writes the entire record to the database.
    /// </summary>
    /// <param name="record">The <see cref="Record"/> to write to the database.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task WriteAsync(Record record)
    {
        List<string> rowsList = [..record.Rows.Select(r => r.ToString())];
        rowsList = rowsList.Prepend("[").ToList();
        rowsList.Add("]");
        await _fs.WriteToRecordFileAsync(_dbName, record.Name, FileExtension.RECORD, rowsList);
    }
}