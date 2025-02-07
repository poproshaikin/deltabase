using Enums.Records.Columns;

namespace Data.Models;

public struct PageRow
{
    internal uint RId { get; }
    
    public string?[] Data { get; set; }
    
    internal uint ValuesCount => (uint)Data.Length; 
    
    public string? this[int index] => Data[index];

    public PageRow(int rid, string?[] data)
    {
        RId = (uint)rid;
        Data = data;
    }
    
    public override string ToString()
    {
        return string.Join("■ ", Data);
    }
}