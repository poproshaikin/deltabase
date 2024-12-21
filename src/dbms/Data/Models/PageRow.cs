namespace Data.Models;

public class PageRow
{
    internal int RId { get; set; }
    
    public string[] Data { get; set; }

    public int ColumnsCount => Data.Length;
    
    // Including the position for a columns count
    internal int ValuesCount => Data.Length + 1; 
    
    public string this[int index] => Data[index];

    internal PageRow(int rid, string[] data)
    {
        RId = rid;
        Data = data;
    }

    public override string ToString()
    {
        return string.Join("■ ", Data);
    }
}