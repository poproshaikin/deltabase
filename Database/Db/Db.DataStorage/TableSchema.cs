using Enums;
using Enums.FileSystem;
using Enums.Records.Columns;

namespace Db.DataStorage;

public class TableSchema
{
    public string TableName { get; private set; }
    
    public string RecordPath { get; private set; }
    
    public ColumnSchema[] Columns { get; private set; }

    public TableSchema(string tableName, string path, IEnumerable<ColumnSchema> columns)
    {
        TableName = tableName;
        RecordPath = path;
        Columns = columns.ToArray();
    }
    
    public ColumnSchema? PrimaryKey => Columns.FirstOrDefault(c => c.HasConstraint(ColumnConstraint.Pk));

    public string GetColumnPath(ColumnSchema column) => GetColumnPath(column.Name);

    public string GetColumnPath(string columnName)
    {
        return $"{RecordPath}/{columnName}.{EnumsStorage.GetExtensionString(FileExtension.RECORD)}";
    }
}