using System.Security;
using Enums.FileSystem;
using Enums.Network;
using Enums.Records.Columns;
using Enums.Sql;
using Enums.Sql.Tokens;

namespace Enums;

public static class EnumsStorage
{
    public const string RecordNullValue = "<null>";
    
    public static readonly Dictionary<string[], TokenType> TokensToTokenTypeMap =
        new Dictionary<string[], TokenType>()
        {
            {
                [ "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "TABLE", "INTO", "SET", "FROM", "VALUES", "WHERE", "LIMIT" ],
                TokenType.Keyword
            },
            {
                [ "+", "-", "*", "/", "=", "==", "<", ">", "<=", ">=", "AND", "OR", "NOT" ], 
                TokenType.Operator
            },
            {
                [ " ", ",", ";", "(", ")" ], 
                TokenType.Separator
            },
            {
                [ "PK", "AI", "NN", "UN"],
                TokenType.Constraint
            },
            {
                [ "NULL", "INTEGER", "CHAR", "FLOAT", "STRING", "INTEGER[]", "CHAR[]", "FLOAT[]", "STRING[]" ],
                TokenType.ValueType
            }
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

    public static readonly Dictionary<string, Keyword> KeywordToTokenMap =
        new Dictionary<string, Keyword>()
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

    public static readonly Dictionary<string, SqlValueType> ValueTypesToTokenMap =
        new Dictionary<string, SqlValueType>()
        {
            { "NULL", SqlValueType.Null },

            { "INTEGER", SqlValueType.Integer },
            { "STRING", SqlValueType.String },
            { "CHAR", SqlValueType.Char },
            { "FLOAT", SqlValueType.Float },

            { "INTEGER[]", SqlValueType.IntegerArr },
            { "STRING[]", SqlValueType.StringArr },
            { "CHAR[]", SqlValueType.CharArr },
            { "FLOAT[]", SqlValueType.FloatArr },
        };

    public static readonly Dictionary<string, ColumnConstraint> ColumnConstraintToTokenMap =
        new Dictionary<string, ColumnConstraint>()
        {
            { "PK", ColumnConstraint.Pk },
            { "AI", ColumnConstraint.Ai },
            { "NN", ColumnConstraint.Nn },
            { "UN", ColumnConstraint.Un },
        };

    public static readonly Dictionary<string, FileType> ExtensionToStringMap =
        new Dictionary<string, FileType>()
        {
            { "def", FileType.Def },
            { "conf", FileType.Conf },
            { "record", FileType.Record },
        };

    public static readonly Dictionary<string, RequestType> CommandToStringMap =
        new Dictionary<string, RequestType>()
        {
            { "sql", RequestType.sql },
            { "connect", RequestType.connect },
            { "close", RequestType.close },
            { "test", RequestType.test },
            { "createdatabase", RequestType.createdatabase },
        };

    public static readonly Dictionary<string, QueryExecutingOption> ExecutingOptionToStringMap =
        new Dictionary<string, QueryExecutingOption>()
        {
            { "non_query", QueryExecutingOption.NonQuery },
            { "scalar", QueryExecutingOption.Scalar },
            { "reader", QueryExecutingOption.Reader }
        };
    
    public static SqlValueType GetColumnValueType(string lexeme)
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

    public static QueryExecutingOption GetExecutingOption(string executingOption)
    {
        return ExecutingOptionToStringMap[executingOption];
    }
    
    public static RequestType GetCommandType(string command)
    {
        return CommandToStringMap[command];
    }

    public static bool ContainsLexeme(string lexeme)
    {
        return TokensToTokenTypeMap.Any(kvp =>
            kvp.Key.Any(token => token == lexeme));
    }
}