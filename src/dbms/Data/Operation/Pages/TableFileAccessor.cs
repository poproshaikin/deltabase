namespace Data.Operation;

internal abstract class TableFileAccessor
{
    protected DataPageManager Manager { get; init; }

    protected TableFileAccessor(DataPageManager manager)
    {
        Manager = manager;
    }
}