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

file static class ArrayExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="items"></param>
    /// <param name="searched"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public static int IndexOf(this string[] items, string? searched)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == searched)
            {
                return i;
            }
        }

        throw new InvalidDataException("Given item wasn't found in the collection");
    }
}