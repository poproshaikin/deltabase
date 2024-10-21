using Enums.Exceptions;

namespace Deltabase.Driver;

public class DltException : Exception
{
    public ErrorType? Error;
    
    public DltException()
    {
    }

    public DltException(ErrorType error)
    {
        Error = error;
    }

    public DltException(string? message) : base(message)
    {
    }

    public DltException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}