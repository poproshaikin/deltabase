namespace Enums.Sql.Tokens;

public enum TokenType
{
    Keyword = 1,
    
    Identifier,
    TableIdentifier,
    ColumnIdentifier,
    
    Operator,
    
    StringLiteral,
    NumberLiteral,
    
    Separator,
    
    Comment,
    Constraint,
    
    ValueType
}