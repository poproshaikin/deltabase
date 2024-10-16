namespace Deltabase.Driver;

public class DltException : Exception
{
    public DltException()
    {
    }

    public DltException(string? message) : base(message)
    {
    }

    public DltException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}