using Enums;
using Enums.Sql.Tokens;

namespace Sql.Tokens;

public class SqlToken
{
    public TokenType Type { get; set; }
    public string Value { get; set; }

    public SqlToken(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public static implicit operator string(SqlToken token) => token.Value;

    public bool IsKeyword(Keyword type)
    {
        if (!IsType(TokenType.Keyword))
            return false;
        
        return EnumsStorage.GetKeyword(Value) == type;
    }

    public bool IsSeparator(SeparatorType type)
    {
        if (!IsType(TokenType.Separator))
            return false;

        return EnumsStorage.GetSeparatorType(Value) == type;
    }

    public bool IsLiteral() => IsType(TokenType.StringLiteral) || IsType(TokenType.NumberLiteral);

    public bool IsOperator() => IsType(TokenType.Operator);

    public bool IsOperator(OperatorType type)
    {
        if (!IsType(TokenType.Operator))
            return false;

        if (type == OperatorType.LogicalOperator)
            return IsOperator(OperatorType.And) ||
                   IsOperator(OperatorType.Or) ||
                   IsOperator(OperatorType.Not);

        return EnumsStorage.GetOperatorType(Value) == type;
    }

    public bool IsType(TokenType type)
    {
        return Type == type;
    }
}