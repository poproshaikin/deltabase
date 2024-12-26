using Enums.Records.Columns;

namespace Data.Models;

public class PageRow
{
    internal int RId { get; set; }
    
    public object[] Data { get; set; }

    public int ColumnsCount => Data.Length;
    
    internal int ValuesCount => Data.Length; 
    
    public object this[int index] => Data[index];

    public PageRow(int rid, object[] data)
    {
        RId = rid;
        Data = data;
    }

    public override string ToString()
    {
        return string.Join("■ ", Data);
    }
}