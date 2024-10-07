namespace Db.Records;

public class RecordRow
{
    private List<string> _values;
    public string[] Values => _values.ToArray();

    public RecordRow() : this(collection: new List<string>())
    {
    }

    public RecordRow(params string[] values) : this(collection: values)
    {
    }

    public void SetValues(IReadOnlyList<string> values)
    {
        _values = values is List<string> list ? list : values.ToList();
    }

    public void AddValue(string value)
    {
        _values.Add(value);
    }

    public void PrependValue(string value)
    {
        _values = _values.Prepend(value).ToList();
    }

    public RecordRow(IReadOnlyList<string> collection)
    {
        SetValues(collection);
    }

    public string this[int index]
    {
        get => Values[index];
        set => _values[index] = value;
    }

    public string[] this[IEnumerable<int> indexes] => indexes.Select(x => Values[x]).ToArray();

    public override string ToString() => string.Join("■ ", Values);
}