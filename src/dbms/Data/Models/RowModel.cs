namespace Data.Models;

public class RowModel
{
    internal int RId { get; set; }
    
    public string[] Data { get; set; }
    public int ValuesCount => Data.Length;

    internal RowModel(int rid, string[] data)
    {
        RId = rid;
        Data = data;
    }

    public override string ToString()
    {
        return string.Join("■ ", Data);
    }
}