using Enums.Exceptions;

namespace Exceptions;

public class DbEngineException : Exception
{
    public ErrorType Error;
    public DbEngineException(ErrorType error)
    {
        Error = error;
    }
}