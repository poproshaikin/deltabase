using System.Diagnostics.CodeAnalysis;
using Enums.Exceptions;

namespace Exceptions;

public class DbEngineException : Exception
{
    public ErrorType Error;
    public DbEngineException(ErrorType error, string? message = null) : base(message)
    {
        Error = error;
    }

    public DbEngineException(string message) : this(ErrorType.InternalServerError, message)
    {
    }
}