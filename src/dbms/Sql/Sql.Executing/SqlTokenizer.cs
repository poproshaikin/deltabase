using Enums;
using Enums.Sql.Tokens;
using Sql.Parsing.Shared;
using Sql.Shared.Tokens;

namespace Sql.Executing.App;

public class SqlTokenizer
{
    public SqlToken[] Tokenize(string rawSql)
    {
        return ToTokens(rawSql);
    }

    /// <summary>
    /// Converts a SQL query string into an array of tokens.
    /// </summary>
    /// <param name="rawSql">The SQL query string to tokenize.</param>
    /// <returns>An array of <see cref="SqlToken"/> representing the tokens in the SQL string.</returns>
    private SqlToken[] ToTokens(string rawSql)
    {
        List<SqlToken> tokenList = [];
        
        string[] lexems = rawSql.Split(SeparatorType.Whitespace, SeparatorType.Comma, SeparatorType.LeftParenthesis,
            SeparatorType.RightParenthesis);

        foreach (string lexeme in lexems)
        {
            // TODO здесь будет проблема с обработкой литералов с пробелами, надо будет это как то обойти
            
            if (EnumsStorage.TryGetTokenType(lexeme, out TokenType type))
            {
                tokenList.Add(new SqlToken(type, lexeme));
            }
            else
            {
                tokenList.Add(ParseLiteral(lexeme));
            }
        }

        return tokenList.ToArray();
    }
    
    /// <summary>
    /// Parses a literal token from a string representation.
    /// </summary>
    /// <param name="literal">The string representation of the literal.</param>
    /// <returns>A <see cref="SqlToken"/> representing the parsed literal.</returns>
    private SqlToken ParseLiteral(string literal)
    {
        if (literal[0] == '\'' && literal[^1] == '\'')
        {
            return new SqlToken(TokenType.StringLiteral, literal[1..^1]);
        }
        else if (literal[0].IsNumeric() && literal.IsNumeric())
        {
            return new SqlToken(TokenType.NumberLiteral, literal);
        }
        else
        {
            return new SqlToken(TokenType.Identifier, literal);
        }
    }
}