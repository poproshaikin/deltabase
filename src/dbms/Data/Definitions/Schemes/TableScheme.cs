using Enums.FileSystem;
using Enums.Records.Columns;

namespace Data.Definitions.Schemes;

public class TableScheme
{
    public string TableName { get; set; }
    
    public ColumnScheme[] Columns { get; set; }

    public ColumnScheme? PrimaryKey => Columns.FirstOrDefault(c => c.HasConstraint(ColumnConstraint.Pk));

    public TableScheme(string tableName, IReadOnlyList<ColumnScheme> columns)
    {
        TableName = tableName;
        Columns = columns.ToArray();
    }
    
    public bool HasColumn(string columnName)
    {
        return GetColumn(columnName) is not null;
    }

    public ColumnScheme? GetColumn(string columnName)
    {
        return Columns.FirstOrDefault(c => c.Name == columnName);
    }
}