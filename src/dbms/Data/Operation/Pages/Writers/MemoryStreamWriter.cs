namespace Data.Operation;

internal class MemoryStreamWriter : PageWriter
{
    protected override MemoryStream Stream { get; }
    
    internal MemoryStreamWriter(byte[] buffer, DataPageManager manager) : base(manager)
    {
        Stream = new MemoryStream(buffer);
    }

    internal static MemoryStreamWriter OpenFromFile(PageInfo pageInfo, DataPageManager manager)
    {
        byte[] buffer = File.ReadAllBytes(pageInfo.Path);
        return new MemoryStreamWriter(buffer, manager);
    }
}