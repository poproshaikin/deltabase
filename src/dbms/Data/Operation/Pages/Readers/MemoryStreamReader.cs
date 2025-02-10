namespace Data.Operation;

internal class MemoryStreamReader : PageReader
{
    protected override MemoryStream Stream { get; }
    
    internal MemoryStreamReader(byte[] buffer, DataPageManager manager) : base(manager)
    {
        Stream = new MemoryStream(buffer);
    }

    internal static MemoryStreamReader OpenFromFile(PageInfo fileMetadata, DataPageManager manager)
    {
        byte[] buffer = File.ReadAllBytes(fileMetadata.Path);
        return new MemoryStreamReader(buffer, manager);
    }
}