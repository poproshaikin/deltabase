namespace Db.Records;

/// <summary>
/// Represents a single row in a record, containing a collection of string values.
/// </summary>
public class RecordRow
{
    private List<string> _values;
    
    /// <summary>
    /// Gets the values in the row as an array.
    /// </summary>
    public string[] Values => _values.ToArray();

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordRow"/> class with an empty collection of values.
    /// </summary>
    public RecordRow() : this(collection: new List<string>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordRow"/> class with the specified values.
    /// </summary>
    /// <param name="values">An array of string values to initialize the row with.</param>
    public RecordRow(params string[] values) : this(collection: values)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordRow"/> class with the specified collection of values.
    /// </summary>
    /// <param name="collection">A read-only list of string values to initialize the row with.</param>
    public RecordRow(IReadOnlyList<string> collection)
    {
        SetValues(collection);
    }

    /// <summary>
    /// Sets the values of the row from a read-only list of strings.
    /// </summary>
    /// <param name="values">The read-only list of string values to set.</param>
    public void SetValues(IReadOnlyList<string> values)
    {
        _values = values is List<string> list ? list : values.ToList();
    }

    /// <summary>
    /// Adds a value to the end of the row.
    /// </summary>
    /// <param name="value">The string value to add.</param>
    public void AddValue(string value)
    {
        _values.Add(value);
    }

    /// <summary>
    /// Prepends a value to the beginning of the row.
    /// </summary>
    /// <param name="value">The string value to prepend.</param>
    public void PrependValue(string value)
    {
        _values = _values.Prepend(value).ToList();
    }

    /// <summary>
    /// Gets or sets the value at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the value to get or set.</param>
    /// <returns>The value at the specified index.</returns>
    public string this[int index]
    {
        get => Values[index];
        set => _values[index] = value;
    }

    /// <summary>
    /// Gets the values at the specified indexes.
    /// </summary>
    /// <param name="indexes">An enumerable collection of zero-based indexes.</param>
    /// <returns>An array of values at the specified indexes.</returns>
    public string[] this[IEnumerable<int> indexes] => indexes.Select(x => Values[x]).ToArray();

    /// <summary>
    /// Returns a string representation of the row, joining the values with a separator.
    /// </summary>
    /// <returns>A string that represents the row.</returns>
    public override string ToString() => string.Join("■ ", Values);
}