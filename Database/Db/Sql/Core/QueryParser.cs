using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Enums.Sql.Tokens;
using Exceptions;
using Sql.Common.Queries;
using Sql.Expressions;
using Sql.Tokens;

namespace Sql.Core;

public class QueryParser
{
    public SqlQuery Parse(string sql)
    {
        SqlToken[] tokens = ToTokens(sql);
        return ToQuery(tokens);
    }

    private SqlToken[] ToTokens(string sql)
    {
        List<SqlToken> tokenList = new();
        
        string[] lexems = sql.Split(SeparatorType.Whitespace, SeparatorType.Comma, SeparatorType.LeftParenthesis,
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

    private SqlQuery ToQuery(SqlToken[] tokens)
    {
        return tokens[0] switch
        {
            { } t when t.IsKeyword(Keyword.Select) => ParseQuery<SelectQuery>(tokens),
            { } t when t.IsKeyword(Keyword.Insert) => ParseQuery<InsertQuery>(tokens),
            { } t when t.IsKeyword(Keyword.Update) => ParseQuery<UpdateQuery>(tokens),
            { } t when t.IsKeyword(Keyword.Delete) => ParseQuery<DeleteQuery>(tokens),

            _ => throw new NotImplementedException()
        };
    }

    private static SqlQuery ParseQuery<TQuery>(SqlToken[] tokens) where TQuery : SqlQuery
    {
        int selectIndex = tokens.IndexOf(Keyword.Select);
        int fromIndex = tokens.IndexOf(Keyword.From);
        int intoIndex = tokens.IndexOf(Keyword.Into);
        int valuesIndex = tokens.IndexOf(Keyword.Values);
        int updateIndex = tokens.IndexOf(Keyword.Update);
        int setIndex = tokens.IndexOf(Keyword.Set);
        int whereIndex = tokens.IndexOf(Keyword.Where);
        int deleteIndex = tokens.IndexOf(Keyword.Delete);
        
        return typeof(TQuery) switch
        {
            { } t when t == typeof(SelectQuery) => new SelectQuery(tokens)
            {
                SelectAllColumns = tokens[selectIndex + 1].IsOperator(OperatorType.Asterisk),
                PassedColumns = tokens[(selectIndex + 1)..fromIndex],
                TableName = tokens[fromIndex + 1],
                Condition = whereIndex is not -1 ? ParseConditionGroup(tokens[(whereIndex + 1)..]) : null
            },

            { } t when t == typeof(InsertQuery) => new InsertQuery(tokens)
            {
                TableName = tokens[intoIndex + 1],
                PassedColumns = tokens[(intoIndex + 2)..valuesIndex],
                PassedValues = tokens[(valuesIndex + 1)..]
            },

            { } t when t == typeof(UpdateQuery) => new UpdateQuery(tokens)
            {
                TableName = tokens[updateIndex + 1],
                Assignments = ParseAssignExpression(tokens[(setIndex + 1)..whereIndex]),
                Condition = whereIndex != -1 ? ParseConditionGroup(tokens[(whereIndex + 1)..]) : null,
            },
            
            { } t when t == typeof(DeleteQuery) => new DeleteQuery(tokens)
            {
                TableName = tokens[fromIndex + 1],
                PassedColumns = tokens[(deleteIndex + 1)..fromIndex],
                SelectAllColumns = tokens[deleteIndex + 1].IsOperator(OperatorType.Asterisk),
                Condition = whereIndex != -1 ? ParseConditionGroup(tokens[(whereIndex + 1)..]) : null,
            },
            
            { } t when t == typeof(CreateTableQuery) => new CreateTableQuery(tokens)
            {
                
            },

            _ => throw new NotImplementedException(),
        };
    }

    private static AssignExpr[] ParseAssignExpression(SqlToken[] tokens)
    {
        const int expression_length = 3;
        const int operator_position = 1;

        int assignmentsCount = tokens.Count(tkn => tkn.IsOperator(OperatorType.Assign));

        if (assignmentsCount == 1) // "Identifier1" = "Value1"
        {
            return new[]
            {
                new AssignExpr(tokens)
                {
                    LeftOperand = tokens[0],
                    RightOperand = tokens[^1],
                }
            };
        }
        else // "Identifier1" = "Value1", "Identifier2" = "Value2", ...
        {
            List<AssignExpr> assignments = new();
            int lastPosition = 0;
            for (int i = 0; i < assignmentsCount; i++)
            {
                List<SqlToken> operands = new();
                for (int j = 0; j < expression_length; j++)
                {
                    lastPosition++;
                    if (j == operator_position)
                        continue;
                    operands.Add(tokens[lastPosition]);
                }
                assignments.Add(new AssignExpr(operands)
                {
                    LeftOperand = operands[0],
                    RightOperand = operands[^1],
                });
            }

            return assignments.ToArray();
        }
    }

    private static ConditionGroup? ParseConditionGroup(SqlToken[]? conditionTokens)
    {
        if (conditionTokens is null)
            return null;

        if (conditionTokens.Length == 3)
        {
            return new ConditionGroup(
                subconditions: new [] { new ConditionExpr(conditionTokens) }, 
                logicalOperators: Array.Empty<SqlToken>());
        }

        List<ConditionExpr> conditions = new();
        List<SqlToken> oneConditionTokens = new();
        List<SqlToken> logicalOperators = new();

        for (int i = 0; i < conditionTokens.Length; i++)
        {
            oneConditionTokens.Add(conditionTokens[i]);

            if (conditionTokens[i].IsOperator(OperatorType.LogicalOperator))
            {
                conditions.Add(new ConditionExpr(oneConditionTokens));
                oneConditionTokens.Clear();
                logicalOperators.Add(conditionTokens[i]);
            }
            else if (i == conditionTokens.Length - 1)
            {
                conditions.Add(new ConditionExpr(oneConditionTokens));
            }
        }

        return new ConditionGroup(conditions, logicalOperators);
    }

    private SqlToken ParseLiteral(string literal)
    {
        if (literal[0] == '\'' && literal[^1] == '\'')
        {
            return new SqlToken(TokenType.StringLiteral, literal[1..^1]);
        }
        else if (literal[0].IsNumeric())
        {
            return new SqlToken(TokenType.NumberLiteral, literal);
        }
        else
        {
            return new SqlToken(TokenType.Identifier, literal);
        }
    }
}

internal static partial class SqlCoreExtensions
{
    public static string[] Split(this string s, params SeparatorType[] types)
    {
        string[] separators = types.Select(EnumsStorage.GetSeparatorString).ToArray();
        return s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    public static bool IsNumeric(this char c) => "0123456789.".Contains(c);

    public static int IndexOf(this SqlToken[] tokens, SqlToken? token) => Array.IndexOf(tokens, token);

    public static int IndexOf(this SqlToken[] tokens, Keyword kw) =>
        tokens.IndexOf(tokens.FirstOrDefault(t => t.IsKeyword(kw)));
    
    public static string[] TokenValuesToArray(this SqlToken[] tokens) => tokens.Select(t => t.Value).ToArray(); 
}