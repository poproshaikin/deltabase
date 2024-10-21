namespace Db.DataStorage;

public class RowDataSet
{
    private List<string> _data;
    public IReadOnlyList<string> Data => _data;
    
    public int Count => _data.Count;

    public RowDataSet(IEnumerable<string> data)
    {
        _data = data.ToList();
    }
    
    public string this[int index] => _data[index];
}