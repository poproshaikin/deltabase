namespace Data.Operation;

internal class FileStreamReader : PageReader
{
    protected override FileStream Stream { get; }

    internal FileStreamReader(FileStream stream, DataPageManager manager) : base(manager)
    {
        Stream = stream;
    }

    internal static FileStreamReader OpenFromFile(PageInfo pageInfo, DataPageManager manager)
    {
        var stream =  new FileStream(pageInfo.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new FileStreamReader(stream, manager);
    }
}