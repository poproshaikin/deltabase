using System.Text;
using Enums;
using Enums.Records.Columns;
using Utils;

namespace Db.Records;

public class Record
{
    public string Name { get; private set; }
    public RecordColumnDef[] Columns { get; private set; }

    private List<RecordRow> _rows;
    public RecordRow[] Rows => _rows.ToArray();

    public bool HasPkColumn => PkColumn is not null;
    public RecordColumnDef? PkColumn => 
        Columns.FirstOrDefault(c => c.HasConstraint(ColumnConstraint.Pk));
    public int PkColumnId => Array.IndexOf(Columns, PkColumn);

    public RecordRow this[int y]  => Rows[y];

    public string this[int y, int x]
    {
        get => Rows[y][x];
        set => Rows[y][x] = value;
    }

    public Record(string name)
    {
        Name = name;
    }

    public Record(string name, IEnumerable<RecordRow> rows) : this(name)
    {
        _rows = rows is List<RecordRow> list ? list : rows.ToList();
    }
    
    public int GetColumnId(string columnName)
    {
        string[] names = Columns.Select(c => c.Name).ToArray();
        return Array.IndexOf(names, columnName);
    }
    
    public void SetDefaultValue(int rowId, int colId) =>
        this[rowId, colId] = Columns[colId].HasConstraint(ColumnConstraint.Nn)
            ? string.Empty
            : EnumsStorage.RecordNullValue;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="recordConfig"></param>
    /// <exception cref="InvalidDataException"></exception>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="readedRows"></param>
    /// <exception cref="InvalidDataException"></exception>
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
    /// 
    /// </summary>
    /// <param name="def"></param>
    /// <exception cref="InvalidDataException"></exception>
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

    public byte[] ToBytes() => Encoding.UTF8.GetBytes(ToString());

    // /// <summary>
    // /// Gets the name of the primary key column from the config file
    // /// </summary>
    // /// <param name="tableName">The name of a table</param>
    // /// <returns><see cref="DltRecordColumnDef"/> or null if wasn't found any primary key column</returns>
    // public DltRecordColumnDef? GetPkColumn(string defFilePath)
    // {
    //     string[] columnDefs = File.ReadAllLines(defFilePath);
    //     string? pkDef = columnDefs.FirstOrDefault(c => 
    //         c.Contains(
    //             ParseHelper.ParseConstraint(ColumnConstraint.Pk)));
    //
    //     return pkDef is null ? null : DltRecordColumnDef.Parse(pkDef);
    // }

    public string[] GetColumnData(string columnName)
    {
        int columnIndex = Array.IndexOf(Columns.Select(c => c.Name).ToArray(), columnName);

        return Rows.Select(row => row.Values[columnIndex]).ToArray();
    }

    public string GetColumnData(string columnName, int rowIndex)
    {
        int columnIndex = Array.IndexOf(Columns.Select(c => c.Name).ToArray(), columnName);

        return Rows[rowIndex].Values[columnIndex];
    }

    public RecordRow[] GetColumns(IEnumerable<string> names) => GetColumns(names.ToArray());
    
    public RecordRow[] GetColumns(string[] names)
    {
        int[] columnsIndexes = names.Select(columnName => 
            Array.IndexOf(Columns, 
                Columns.FirstOrDefault(c => c.Name == columnName)))
            .ToArray();

        return Rows.Select(r => new RecordRow(r[columnsIndexes])).ToArray();
    }
    
    /// <summary>
    /// Tries to get a last primary key
    /// </summary>
    /// <param name="value">The value of primary key if success; otherwise - null </param>
    /// <returns>true if success; otherwise - false</returns>
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

    public void AddRow(RecordRow newRow)
    {
        if (CheckRowValidity(newRow))
        {
            _rows.Add(newRow);
        }
    }

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

    public void DeleteRow(int rowId)
    {
        _rows.RemoveAt(rowId);
    }

    private bool CheckRowValidity(RecordRow? newRow)
    {
        if (newRow is null)
            return false;

        if (newRow.Values.Length != Columns.Length)
            return false;

        return true;
    }

    public void Clear()
    {
        _rows = new List<RecordRow>();
    }

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