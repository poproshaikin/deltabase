using System.Security;
using Enums.FileSystem;
using Enums.Network;
using Enums.Records.Columns;
using Enums.Sql;
using Enums.Sql.Tokens;

namespace Enums;

public static class EnumsStorage
{
    public static readonly Dictionary<string, SeparatorType> SeparatorToTokenMap = new()
        {
            { " ", SeparatorType.Whitespace },
            { ",", SeparatorType.Comma },
            { ";", SeparatorType.Semicolon },
            { "(", SeparatorType.LeftParenthesis },
            { ")", SeparatorType.RightParenthesis },
        };

    public static readonly Dictionary<string, Keyword> KeywordToTokenMap = new()
        {
            { "SELECT", Keyword.Select },
            { "INSERT", Keyword.Insert },
            { "UPDATE", Keyword.Update },
            { "DELETE", Keyword.Delete },
            { "CREATE", Keyword.Create },
            { "TABLE", Keyword.Table },
            { "INTO", Keyword.Into },
            { "SET", Keyword.Set },
            { "FROM", Keyword.From },
            { "VALUES", Keyword.Values },
            { "*", Keyword.Asterisk },
            { "WHERE", Keyword.Where },
            { "LIMIT", Keyword.Limit },
        };

    public static readonly Dictionary<string, OperatorType> OperatorToTokenMap = new()
        {
            { "*", OperatorType.Asterisk },
            { "=", OperatorType.Assign },
            { "==", OperatorType.Equals },
            { "!=", OperatorType.NotEquals },
            { "<", OperatorType.LessThan },
            { ">", OperatorType.GreaterThan },
            { "<=", OperatorType.LessThanOrEquals },
            { ">=", OperatorType.GreaterThanOrEquals },
            { "AND", OperatorType.And },
            { "OR", OperatorType.Or },
            { "NOT", OperatorType.Not },
        };

    public static readonly Dictionary<string, ColumnConstraint> ColumnConstraintToTokenMap = new()
        {
            { "PK", ColumnConstraint.Pk },
            { "AI", ColumnConstraint.Ai },
            { "NN", ColumnConstraint.Nn },
            { "UN", ColumnConstraint.Un },
        };

    public static readonly Dictionary<string, FileType> ExtensionToStringMap = new()
        {
            { "def", FileType.Def },
            { "conf", FileType.Conf },
            { "record", FileType.Record },
        };

    public static readonly Dictionary<string, RequestType> CommandToStringMap = new()
    {
        { "sql", RequestType.sql },
        { "connect", RequestType.connect },
        { "close", RequestType.close },
        { "test", RequestType.test },
        { "createdatabase", RequestType.createdatabase },
    };
    
    public static readonly Dictionary<string, TokenType> TokensToTokenTypeMap =
        KeywordToTokenMap.Keys.ToDictionary(k => k, _ => TokenType.Keyword)
            .Concat(OperatorToTokenMap.Keys.ToDictionary(k => k, _ => TokenType.Operator))
            .Concat(SeparatorToTokenMap.Keys.ToDictionary(k => k, _ => TokenType.Separator))
            .Concat(ColumnConstraintToTokenMap.Keys.ToDictionary(k => k, _ => TokenType.Constraint))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public static TokenType GetTokenType(string lexeme)
    {
        return TokensToTokenTypeMap[lexeme];
    }

    public static ColumnConstraint GetConstraint(string lexeme)
    {
        return ColumnConstraintToTokenMap[lexeme];
    }

    public static Keyword GetKeyword(string lexeme)
    {
        return KeywordToTokenMap[lexeme];
    }

    public static SeparatorType GetSeparatorType(string lexeme)
    {
        return SeparatorToTokenMap[lexeme];
    }
    
    public static OperatorType GetOperatorType(string lexeme)
    {
        return OperatorToTokenMap[lexeme];
    }

    public static bool TryGetTokenType(string lexeme, out TokenType type)
    {
        type = GetTokenType(lexeme);
        return type != default;
    }

    public static bool TryGetConstraintType(string lexeme, out ColumnConstraint constraint)
    {
        constraint = GetConstraint(lexeme);
        return constraint != default;
    }

    public static string GetSeparatorString(SeparatorType type)
    {
        return SeparatorToTokenMap.FirstOrDefault(kvp => kvp.Value == type).Key ??
               throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }

    public static string GetKeywordString(Keyword type)
    {
        return KeywordToTokenMap.FirstOrDefault(kvp => kvp.Value == type).Key ??
               throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }
    
    public static string GetExtensionString(FileType type)
    {
        return ExtensionToStringMap.FirstOrDefault(kvp => kvp.Value == type).Key ??
               throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }
    
    public static RequestType GetCommandType(string command)
    {
        return CommandToStringMap[command];
    }
}