using System.Data;

namespace Db.DataStorage;

public class TableDataSet
{
    private List<ColumnSchema> _attributes;
    public IReadOnlyList<ColumnSchema> Attributes => _attributes;

    private List<ColumnDataSet> _columnsData;
    public IReadOnlyList<ColumnDataSet> ColumnsData => _columnsData;

    private IEnumerable<RowDataSet> _rows; // access it through Rows property
    private bool _needToUpdateRows;
    public IReadOnlyList<RowDataSet> Rows
    {
        get
        {
            if (_needToUpdateRows)
            {
                _rows = DataUtils.ToRows(_columnsData);
                _needToUpdateRows = false;
            }

            return _rows.ToArray();
        }
        set
        {
            _needToUpdateRows = true;
            _rows = value;
        }
    }

    public int RowsCount => Rows.Count;

#region Constructors
    
    public TableDataSet(IReadOnlyList<ColumnSchema> attributes, IReadOnlyList<ColumnDataSet> columnsData)
    {
        _attributes = attributes.ToList();
        _columnsData = columnsData.ToList();
    }

    public TableDataSet(IReadOnlyList<ColumnSchema> attributes, IReadOnlyList<RowDataSet> rows)
    {
        _attributes = attributes.ToList();
        _columnsData = DataUtils.ToColumns(rows).ToList();
    }

    public TableDataSet(TableSchema schema, IReadOnlyList<ColumnDataSet> columnsData)
    {
        _attributes = schema.Columns.ToList();
        _columnsData = columnsData.ToList();
    }
    
#endregion

    public int IndexOfColumn(string columnName)
    {
        ColumnSchema targetColumn = _attributes.FirstOrDefault(c => c.Name == columnName)!;
        return IndexOfColumn(targetColumn);
    }
    public int IndexOfColumn(ColumnSchema column) => _attributes.IndexOf(column);

    public ColumnDataSet? GetColumnData(ColumnSchema column)
    {
        int columnIndex = _attributes.IndexOf(column);
        return _columnsData[columnIndex];
    }
}