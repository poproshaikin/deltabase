using Enums.Records.Columns;

namespace Data.Models;

public struct PageRow
{
    internal int? RId { get; }
    
    public string?[] Data { get; set; }
    
    internal int ValuesCount => Data.Length; 
    
    public string? this[int index] => Data[index];

    public PageRow(int rid, string?[] data)
    {
        RId = rid;
        Data = data;
    }
    
    public override string ToString()
    {
        return string.Join("■ ", Data);
    }
}