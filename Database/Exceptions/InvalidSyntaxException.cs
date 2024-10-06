namespace Exceptions;

public class InvalidSyntaxException : Exception
{
    public InvalidSyntaxException() : base()
    {
    }

    public InvalidSyntaxException(string message) : base(message)
    {
    }

    public static InvalidSyntaxException InvalidSql(params string[] invalidTokens) =>
        new InvalidSyntaxException($"Invalid SQL command syntax: {string.Join('\n', invalidTokens)}");
    
    public static InvalidSyntaxException UnknownCommand() => 
        new InvalidSyntaxException("Unknown SQL command");
    
    public static InvalidSyntaxException RecordNotFound(string name) =>
        new InvalidSyntaxException($"Record with this name wasn't found: {name}");
    
    public static InvalidSyntaxException InvalidToken(string token) =>
        new InvalidSyntaxException($"Invalid token: {token}");
    
    public static InvalidSyntaxException UnexpectedToken(string? token) 
        => token is null ? 
            new InvalidSyntaxException("Unexpected token") :
            new InvalidSyntaxException($"Unexpected token: {token}");
}