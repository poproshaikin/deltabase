using System.Security;
using Enums.Records.Columns;
using Enums.Sql.Tokens;

namespace Enums;

public static class EnumsStorage
{
    public const string RecordNullValue = "<null>";
    
    public static readonly Dictionary<string[], TokenType> TokensToTokenTypeMap =
        new Dictionary<string[], TokenType>()
        {
            {
                new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "TABLE", "INTO", "SET", "FROM", "VALUES", "WHERE" },
                TokenType.Keyword
            },
            { new[] { "+", "-", "*", "/", "=", "==", "<", ">", "<=", ">=", "AND", "OR", "NOT" }, TokenType.Operator },
            { new[] { " ", ",", ";", "(", ")" }, TokenType.Separator },
        };

    public static readonly Dictionary<string, SeparatorType> SeparatorToTokenMap =
        new Dictionary<string, SeparatorType>()
        {
            { " ", SeparatorType.Whitespace },
            { ",", SeparatorType.Comma },
            { ";", SeparatorType.Semicolon },
            { "(", SeparatorType.LeftParenthesis },
            { ")", SeparatorType.RightParenthesis },
        };

    public static readonly Dictionary<string, KeywordType> KeywordToTokenMap =
        new Dictionary<string, KeywordType>()
        {
            { "SELECT", KeywordType.Select },
            { "INSERT", KeywordType.Insert },
            { "UPDATE", KeywordType.Update },
            { "DELETE", KeywordType.Delete },
            { "CREATE", KeywordType.Create },
            { "TABLE", KeywordType.Table },
            { "INTO", KeywordType.Into },
            { "SET", KeywordType.Set },
            { "FROM", KeywordType.From },
            { "VALUES", KeywordType.Values },
            { "*", KeywordType.Asterisk },
            { "WHERE", KeywordType.Where },
        };

    public static readonly Dictionary<string, OperatorType> OperatorToTokenMap =
        new Dictionary<string, OperatorType>()
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

    public static readonly Dictionary<string, ColumnValueType> ValueTypesToTokenMap =
        new Dictionary<string, ColumnValueType>()
        {
            { "NULL", ColumnValueType.Null },

            { "INTEGER", ColumnValueType.Integer },
            { "STRING", ColumnValueType.String },
            { "CHAR", ColumnValueType.Char },
            { "FLOAT", ColumnValueType.Float },

            { "INTEGER[]", ColumnValueType.IntegerArr },
            { "STRING[]", ColumnValueType.StringArr },
            { "CHAR[]", ColumnValueType.CharArr },
            { "FLOAT[]", ColumnValueType.FloatArr },
        };
    
    public static ColumnValueType GetColumnValueType(string lexeme)
    {
        return ValueTypesToTokenMap[lexeme];
    }

    public static TokenType GetTokenType(string lexeme)
    {
        string[]? foundLexemes = TokensToTokenTypeMap.Keys.FirstOrDefault(s => s.Contains(lexeme));

        return foundLexemes switch
        {
            null => default,
            not null => TokensToTokenTypeMap[foundLexemes],
        };
    }

    public static KeywordType GetKeywordType(string lexeme)
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

    public static string GetSeparatorString(SeparatorType type)
    {
        return SeparatorToTokenMap.FirstOrDefault(kvp => kvp.Value == type).Key ??
               throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }

    public static string GetKeywordString(KeywordType type)
    {
        return KeywordToTokenMap.FirstOrDefault(kvp => kvp.Value == type).Key ??
               throw new ArgumentOutOfRangeException(nameof(type), type, null);

    }

    public static bool ContainsLexeme(string lexeme)
    {
        return TokensToTokenTypeMap.Any(kvp =>
            kvp.Key.Any(token => token == lexeme));
    }
}