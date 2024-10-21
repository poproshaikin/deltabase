namespace Db.DataStorage;

public class ColumnDataSet
{
    private List<string> _data;
    public IReadOnlyList<string> Data => _data;
    
    public int RowsCount => Data.Count;

    public ColumnDataSet(IEnumerable<string> data)
    {
        _data = data.ToList();
    }
    
    public string this[int index] => Data[index];
}