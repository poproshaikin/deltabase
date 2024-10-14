using System.Text;
using Enums;
using Enums.Records.Columns;
using Utils;

namespace Db.Records;

/// <summary>
/// Represents a record in a database table, managing its columns and rows.
/// </summary>
public class Record
{
    /// <summary>
    /// Gets the name of the record.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Gets the array of column definitions for this record.
    /// </summary>
    public RecordColumnDef[] Columns { get; private set; }

    private List<RecordRow> _rows;
    
    /// <summary>
    /// Gets the array of rows in the record.
    /// </summary>
    public RecordRow[] Rows => _rows.ToArray();

    /// <summary>
    /// Gets a value indicating whether the record has a primary key column.
    /// </summary>
    public bool HasPkColumn => PkColumn is not null;
    
    /// <summary>
    /// Gets the primary key column definition, if available.
    /// </summary>
    public RecordColumnDef? PkColumn => 
        Columns.FirstOrDefault(c => c.HasConstraint(ColumnConstraint.Pk));
    
    /// <summary>
    /// Gets the index of the primary key column.
    /// </summary>
    public int PkColumnId => Array.IndexOf(Columns, PkColumn);

    /// <summary>
    /// Indexer to access a specific row by its index.
    /// </summary>
    /// <param name="y">The index of the row.</param>
    public RecordRow this[int y]  => Rows[y];

    /// <summary>
    /// Indexer to access a specific cell in a row.
    /// </summary>
    /// <param name="y">The row index.</param>
    /// <param name="x">The column index.</param>
    public string this[int y, int x]
    {
        get => Rows[y][x];
        set => Rows[y][x] = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Record"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the record.</param>
    public Record(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Record"/> class with the specified name and rows.
    /// </summary>
    /// <param name="name">The name of the record.</param>
    /// <param name="rows">The initial rows of the record.</param>
    public Record(string name, IEnumerable<RecordRow> rows) : this(name)
    {
        _rows = rows is List<RecordRow> list ? list : rows.ToList();
    }
    
    /// <summary>
    /// Gets the index of the specified column name.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>The index of the column, or -1 if not found.</returns>
    public int GetColumnId(string columnName)
    {
        string[] names = Columns.Select(c => c.Name).ToArray();
        return Array.IndexOf(names, columnName);
    }
    
    /// <summary>
    /// Sets the default value for the specified cell in the record.
    /// </summary>
    /// <param name="rowId">The row index.</param>
    /// <param name="colId">The column index.</param>
    public void SetDefaultValue(int rowId, int colId) =>
        this[rowId, colId] = Columns[colId].HasConstraint(ColumnConstraint.Nn)
            ? string.Empty
            : EnumsStorage.RecordNullValue;

    /// <summary>
    /// Parses the record definition and rows asynchronously from the provided tasks.
    /// </summary>
    /// <param name="rows">A task that represents the rows to parse.</param>
    /// <param name="defs">A task that represents the definitions to parse.</param>
    /// <param name="recordName">The name of the record.</param>
    /// <exception cref="InvalidDataException">Thrown if the data is invalid.</exception>
    /// <returns>A new <see cref="Record"/> instance.</returns>
    public static Record ParseAndAwaitAsync(Task<string[]> rows, Task<string[]> defs, string recordName)
    {
        var record = new Record(recordName);
        
        try
        {
            Task.WaitAll(rows, defs);
        }
        catch (DirectoryNotFoundException ex)
        {
            ThrowHelper.ThrowRecordNotFound(recordName);
        }
        
        record.ParseAndSetDef(defs.Result);
        record.ParseAndSetRows(rows.Result);
        return record;
    }
    
    /// <summary>
    /// Parses and sets the rows from the provided string array.
    /// </summary>
    /// <param name="readedRows">The rows to parse.</param>
    /// <exception cref="InvalidDataException">Thrown if the data is invalid.</exception>
    private void ParseAndSetRows(string[] readedRows)
    {
        if (readedRows[0] == "[" && readedRows[^1] == "]")
        {
            readedRows = readedRows[1..^1];
        }
        else 
            throw new InvalidDataException();

        var rows = new List<RecordRow>();
        rows.AddRange(readedRows.Select(s =>
        {
            string[] splitted = s.Split("■ ");
            if (splitted.Length != Columns.Length) throw new InvalidDataException();
            return new RecordRow(splitted);
        }));

        _rows = rows;
    }

    /// <summary>
    /// Parses and sets the column definitions from the provided string array.
    /// </summary>
    /// <param name="def">The column definitions to parse.</param>
    /// <exception cref="InvalidDataException">Thrown if the data is invalid.</exception>
    private void ParseAndSetDef(string[] def)
    {
        var columns = new List<RecordColumnDef>();
        
        foreach (string columnDef in def)  
        {
            string[] splitted = columnDef.Split(' ');
            if (splitted.Length < 2) throw new InvalidDataException();

            string name = splitted[0];
            string valueType = splitted[1];
            string[] constraints = Array.Empty<string>();
            
            if (splitted.Length > 2)
            {
                constraints = splitted[2..];
            }

            var column = new RecordColumnDef(name, valueType, constraints);
            columns.Add(column);
        }

        Columns = columns.ToArray();
    }

    /// <summary>
    /// Converts the record to a byte array.
    /// </summary>
    /// <returns>A byte array representation of the record.</returns>
    public byte[] ToBytes() => Encoding.UTF8.GetBytes(ToString());

    /// <summary>
    /// Gets the data for a specified column across all rows.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>An array of strings containing the data for the column.</returns>
    public string[] GetColumnData(string columnName)
    {
        int columnIndex = Array.IndexOf(Columns.Select(c => c.Name).ToArray(), columnName);

        return Rows.Select(row => row.Values[columnIndex]).ToArray();
    }

    /// <summary>
    /// Gets the data for a specified column at a specific row index.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="rowIndex">The index of the row.</param>
    /// <returns>The value of the specified cell.</returns>
    public string GetColumnData(string columnName, int rowIndex)
    {
        int columnIndex = Array.IndexOf(Columns.Select(c => c.Name).ToArray(), columnName);

        return Rows[rowIndex].Values[columnIndex];
    }

    /// <summary>
    /// Gets a subset of columns by their names.
    /// </summary>
    /// <param name="names">The names of the columns to retrieve.</param>
    /// <returns>An array of <see cref="RecordRow"/> containing the specified columns.</returns>
    public RecordRow[] GetColumns(IEnumerable<string> names) => GetColumns(names.ToArray());
    
    /// <summary>
    /// Gets a subset of columns by their names.
    /// </summary>
    /// <param name="names">The names of the columns to retrieve.</param>
    /// <returns>An array of <see cref="RecordRow"/> containing the specified columns.</returns>
    public RecordRow[] GetColumns(string[] names)
    {
        int[] columnsIndexes = names.Select(columnName => 
            Array.IndexOf(Columns, 
                Columns.FirstOrDefault(c => c.Name == columnName)))
            .ToArray();

        return Rows.Select(r => new RecordRow(r[columnsIndexes])).ToArray();
    }

    /// <summary>
    /// Checks if the collection of columns contains a column with the specified name.
    /// </summary>
    /// <param name="columnName">The name of the column to check for.</param>
    /// <returns>
    /// <c>true</c> if the collection contains a column with the specified name; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsColumn(string columnName) => 
        Columns.Any(c => c.Name == columnName);

    /// <summary>
    /// Tries to get the last primary key value from the record if primary key column exists.
    /// </summary>
    /// <param name="value">The value of the primary key if successful; otherwise, null.</param>
    /// <returns>true if successful; otherwise, false.</returns>
    public bool TryGetLastPkValue(out string value)
    {
        int pkIndex = -1;
        
        for (int i = 0; i < Columns.Length; i++)
            if (Columns[i].HasConstraint(ColumnConstraint.Pk))
                pkIndex = i;
        try
        {
            value = Rows[^1].Values[pkIndex];
            return true;
        }
        catch (IndexOutOfRangeException)
        {
            value = null!;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the last primary key value from the record if primary key column exists as an integer.
    /// </summary>
    /// <param name="value">The int representation of the value of the primary key if successful; otherwise, null.</param>
    /// <returns>true if successful; otherwise, false.</returns>
    public bool TryGetLastPkValueInt(out int? value)
    {
        bool succeed = TryGetLastPkValue(out string valueString);

        if (succeed)
        {
            value = int.Parse(valueString);
        }
        else
        {
            value = null;
        }
        
        return succeed;
    }

    /// <summary>
    /// Adds a new row to the record.
    /// </summary>
    /// <param name="newRow">The row to add.</param>
    public void AddRow(RecordRow newRow)
    {
        if (CheckRowValidity(newRow))
        {
            _rows.Add(newRow);
        }
    }

    /// <summary>
    /// Adds a new row to the record and orders it based on the primary key.
    /// </summary>
    /// <param name="newRow">The row to add.</param>
    public void AddRowAndOrder(RecordRow newRow)
    {
        if (!HasPkColumn)
        {
            AddRow(newRow);
        }
        else
        {
            int newRowPk = int.Parse(newRow[PkColumnId]);

            if (newRowPk < _rows.Count)
            {
                if (CheckRowValidity(newRow))
                {
                    _rows.Insert(newRowPk, newRow);
                }
            }
            else
            {
                AddRow(newRow);
            }
        }
    }

    /// <summary>
    /// Deletes a row from the record by its index.
    /// </summary>
    /// <param name="rowId">The index of the row to delete.</param>
    public void DeleteRow(int rowId)
    {
        _rows.RemoveAt(rowId);
    }

    /// <summary>
    /// Checks the validity of a row to ensure it matches the expected structure.
    /// </summary>
    /// <param name="newRow">The row to check.</param>
    /// <returns>true if the row is valid; otherwise, false.</returns>
    private bool CheckRowValidity(RecordRow? newRow)
    {
        if (newRow is null)
            return false;

        if (newRow.Values.Length != Columns.Length)
            return false;

        return true;
    }

    /// <summary>
    /// Clears all rows from the record.
    /// </summary>
    public void Clear()
    {
        _rows = new List<RecordRow>();
    }

    /// <summary>
    /// Converts the record to its string representation.
    /// </summary>
    /// <returns>A string representation of the record.</returns>
    public override string ToString()
    {
        const string values_separator = "■ ";
        string @string = "";

        foreach (RecordRow row in Rows)
        {
            for(int i = 0; i < row.Values.Length; i++)
            {
                @string += row[i] + values_separator;

                if (i == row.Values.Length - 1)
                {
                    @string += '\n';
                }
            }
        }

        return @string;
    }
}