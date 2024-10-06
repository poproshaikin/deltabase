using System.Runtime.InteropServices.ComTypes;
using Enums;
using Enums.Sql.Queries;
using Enums.Sql.Tokens;
using Sql.Queries;
using Sql.Tokens;
using Sql;
using Sql.Expressions;
using Utils;

namespace Sql.Core;

public class QueryParser
{
    public SqlQuery ParseQuery(string query)
    {
        SqlToken[] tokens = ToTokens(query);
        return ToQuery(tokens);
    }
    
    private SqlToken[] ToTokens(string query)
    {
        List<SqlToken> tokenList = new();
        
        string[] lexems = query.Split(' ', ',', '(', ')');

        foreach (string lexeme in lexems)
        {
            // TODO здесь будет проблема с обработкой литералов с пробелами, надо будет это как то обойти
            
            if (EnumsStorage.TryGetTokenType(lexeme, out TokenType type))
            {
                tokenList.Add(new SqlToken(type, lexeme));
            }
            else
            {
                tokenList.Add(ToLiteral(lexeme));
            }
        }

        return tokenList.ToArray();
    }

    private SqlQuery ToQuery(SqlToken[] tokens)
    {
        if (tokens[0].Type != TokenType.Keyword)
        {
            throw ThrowHelper.UnknownCommand();
        }

        Keyword kw = EnumsStorage.GetKeyword(tokens[0]);
        return kw switch
        {
            Keyword.Select => ParseDqlQuery(tokens),
            
            _ => throw new NotImplementedException()
        };
    }

    private SqlToken ToLiteral(string lexeme)
    {
        if (lexeme[0] == '\'' && lexeme[^1] == '\'')
        {
            return new SqlToken(TokenType.StringLiteral, lexeme[1..^1]);
        }
        else if (lexeme.IsNumeric())
        {
            return new SqlToken(TokenType.NumberLiteral, lexeme);
        }
        else
        {
            return new SqlToken(TokenType.Identifier, lexeme);
        }
    }

    private DqlQuery ParseDqlQuery(SqlToken[] tokens)
    {
        int selectIndex = tokens.IndexOf(Keyword.Select);
        int fromIndex = tokens.IndexOf(Keyword.From);
        int whereIndex = tokens.IndexOf(Keyword.Where);
        
        SqlExpression[] expressions =
        [
            new SelectExpression(tokens[selectIndex + 1]),
            new FromExpression(tokens[fromIndex + 1])
        ];

        if (whereIndex != 1)
        {
            throw new NotImplementedException();
        }

        return new DqlQuery(expressions, QueryType.Select);
    }
}