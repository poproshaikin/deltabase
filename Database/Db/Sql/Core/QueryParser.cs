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
        
        List<SqlExpression> expressions =
        [
            new SelectExpression(tokens[(selectIndex + 1)..fromIndex]),
            new FromExpression(tokens[fromIndex + 1])
        ];
        
        if (whereIndex != -1)
        {
            expressions.AddRange(ParseConditions(tokens[(whereIndex + 1)..]));
        }

        return new DqlQuery(expressions, QueryType.Select);
    }

    private ConditionExpression[] ParseConditions(SqlToken[] conditionTokens)
    {
        if (conditionTokens.Length == 3)
        {
            return [new ConditionExpression(conditionTokens[0], conditionTokens[1], conditionTokens[2])];
        }
        
        List<ConditionExpression> conditions = [];
        List<SqlToken> oneConditionTokens = [];
        List<SqlToken> logicalOperators = [];

        for (int i = 0; i < conditionTokens.Length; i++)
        {
            oneConditionTokens.Add(conditionTokens[i]);

            if (conditionTokens[i].IsOperator(OperatorType.LogicalOperator))
            {
                conditions.Add(parseCondition(oneConditionTokens));
                oneConditionTokens.Clear();
                logicalOperators.Add(conditionTokens[i]);
            }
            else if (i == conditionTokens.Length - 1)
            {
                conditions.Add(parseCondition(oneConditionTokens));
            }
        }

        return conditions.ToArray();

        ConditionExpression parseCondition(IReadOnlyList<SqlToken> tokens) =>
            new(
                tokens[0],
                tokens[2],
                tokens[1]);
    }
}