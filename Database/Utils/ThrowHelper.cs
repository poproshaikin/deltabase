using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Exceptions;

namespace Utils;

public static class ThrowHelper
{
    public static InvalidSyntaxException InvalidSql(params string[] invalidTokens) =>
        InvalidSyntaxException.InvalidSql(invalidTokens);

    public static InvalidSyntaxException UnknownCommand() =>
        InvalidSyntaxException.UnknownCommand();

    public static InvalidSyntaxException RecordNotFound(string name) =>
        InvalidSyntaxException.RecordNotFound(name);

    public static InvalidSyntaxException InvalidToken(string token) => 
        InvalidSyntaxException.InvalidToken(token);

    private static InvalidSyntaxException UnexpectedToken(string? token) =>
        InvalidSyntaxException.UnexpectedToken(token);

    [DoesNotReturn, StackTraceHidden]
    public static void ThrowRecordNotFound(string recordName) => 
        throw RecordNotFound(recordName);

    [DoesNotReturn, StackTraceHidden]
    public static void ThrowUnknownCommand() => 
        throw UnknownCommand();

    [DoesNotReturn, StackTraceHidden]
    public static void ThrowInvalidSql(params string[] literal) =>
        throw InvalidSql(literal);

    [DoesNotReturn, StackTraceHidden]
    public static void ThrowUnexpectedToken(string? token) =>
        throw UnexpectedToken(token);
}