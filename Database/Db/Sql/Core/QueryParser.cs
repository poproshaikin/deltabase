using Enums;
using Enums.Sql.Tokens;
using Sql.Expressions;
using Sql.Queries;
using Sql.Tokens;

namespace Sql.Core;

/// <summary>
/// Parses SQL queries and converts them into corresponding SQL query objects.
/// </summary>
public class QueryParser
{
    /// <summary>
    /// Parses the provided SQL string and converts it into a <see cref="SqlQuery"/> object.
    /// </summary>
    /// <param name="sql">The SQL query string to parse.</param>
    /// <returns>A <see cref="SqlQuery"/> object representing the parsed SQL.</returns>
    public SqlQuery Parse(string sql)
    {
        SqlToken[] tokens = ToTokens(sql);
        return ToQuery(tokens);
    }

    /// <summary>
    /// Converts a SQL query string into an array of tokens.
    /// </summary>
    /// <param name="sql">The SQL query string to tokenize.</param>
    /// <returns>An array of <see cref="SqlToken"/> representing the tokens in the SQL string.</returns>
    private SqlToken[] ToTokens(string sql)
    {
        List<SqlToken> tokenList = [];
        
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

    /// <summary>
    /// Converts an array of SQL tokens into the corresponding <see cref="SqlQuery"/> object.
    /// </summary>
    /// <param name="tokens">The array of <see cref="SqlToken"/> to convert.</param>
    /// <returns>A <see cref="SqlQuery"/> object representing the parsed tokens.</returns>
    private SqlQuery ToQuery(SqlToken[] tokens)
    {
        return tokens[0] switch
        {
            { } t when t.IsKeyword(Keyword.Select) => ParseSelect(tokens),
            { } t when t.IsKeyword(Keyword.Insert) => ParseInsert(tokens),
            { } t when t.IsKeyword(Keyword.Update) => ParseUpdate(tokens),
            { } t when t.IsKeyword(Keyword.Delete) => ParseDelete(tokens),
            { } t when t.IsKeyword(Keyword.Create) => ParseCreate(tokens),

            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Parses a SELECT query from the provided tokens.
    /// </summary>
    /// <param name="tokens">The array of <see cref="SqlToken"/> representing the SELECT query.</param>
    /// <returns>A <see cref="SelectQuery"/> object representing the parsed SELECT query.</returns>
    private SelectQuery ParseSelect(SqlToken[] tokens)
    {
        int selectIndex = tokens.IndexOf(Keyword.Select);
        int fromIndex = tokens.IndexOf(Keyword.From);
        int whereIndex = tokens.IndexOf(Keyword.Where);

        return new SelectQuery(tokens)
        {
            SelectAllColumns = tokens[selectIndex + 1].IsOperator(OperatorType.Asterisk),
            PassedColumns = tokens[(selectIndex + 1)..fromIndex],
            TableName = tokens[fromIndex + 1],
            Condition = whereIndex is not -1 ? ParseConditionGroup(tokens[(whereIndex + 1)..]) : null
        };
    }

    /// <summary>
    /// Parses an INSERT query from the provided tokens.
    /// </summary>
    /// <param name="tokens">The array of <see cref="SqlToken"/> representing the INSERT query.</param>
    /// <returns>A <see cref="InsertQuery"/> object representing the parsed INSERT query.</returns>
    private InsertQuery ParseInsert(SqlToken[] tokens)
    {
        int intoIndex = tokens.IndexOf(Keyword.Into);
        int valuesIndex = tokens.IndexOf(Keyword.Values);

        return new InsertQuery(tokens)
        {
            TableName = tokens[intoIndex + 1],
            PassedColumns = tokens[(intoIndex + 2)..valuesIndex],
            PassedValues = tokens[(valuesIndex + 1)..]
        };
    }

    /// <summary>
    /// Parses an UPDATE query from the provided tokens.
    /// </summary>
    /// <param name="tokens">The array of <see cref="SqlToken"/> representing the UPDATE query.</param>
    /// <returns>A <see cref="UpdateQuery"/> object representing the parsed UPDATE query.</returns>
    private UpdateQuery ParseUpdate(SqlToken[] tokens)
    {
        int updateIndex = tokens.IndexOf(Keyword.Update);
        int setIndex = tokens.IndexOf(Keyword.Set);
        int whereIndex = tokens.IndexOf(Keyword.Where);

        return new UpdateQuery(tokens)
        {
            TableName = tokens[updateIndex + 1],
            Assignments = ParseAssignExpression(tokens[(setIndex + 1)..whereIndex]),
            Condition = whereIndex != -1 ? ParseConditionGroup(tokens[(whereIndex + 1)..]) : null,
        };
    }

    /// <summary>
    /// Parses a DELETE query from the provided tokens.
    /// </summary>
    /// <param name="tokens">The array of <see cref="SqlToken"/> representing the DELETE query.</param>
    /// <returns>A <see cref="DeleteQuery"/> object representing the parsed DELETE query.</returns>
    private DeleteQuery ParseDelete(SqlToken[] tokens)
    {
        int whereIndex = tokens.IndexOf(Keyword.Where);
        int deleteIndex = tokens.IndexOf(Keyword.Delete);
        int fromIndex = tokens.IndexOf(Keyword.From);

        return new DeleteQuery(tokens)
        {
            TableName = tokens[fromIndex + 1],
            PassedColumns = tokens[(deleteIndex + 1)..fromIndex],
            SelectAllColumns = tokens[deleteIndex + 1].IsOperator(OperatorType.Asterisk),
            Condition = whereIndex != -1 ? ParseConditionGroup(tokens[(whereIndex + 1)..]) : null,
        };
    }
    
    /// <summary>
    /// Parses a CREATE TABLE query from the provided tokens.
    /// </summary>
    /// <param name="tokens">The array of <see cref="SqlToken"/> representing the CREATE TABLE query.</param>
    /// <returns>A <see cref="CreateTableQuery"/> object representing the parsed CREATE TABLE query.</returns>
    private CreateTableQuery ParseCreate(SqlToken[] tokens)
    {
        int tableIndex = tokens.IndexOf(Keyword.Table);
        
        return new CreateTableQuery(tokens)
        {
            TableName = tokens[tableIndex + 1],
            NewColumns = ParseColumnDefExpressions(tokens[(tableIndex + 2)..])
        };
    }

    /// <summary>
    /// Parses assignment expressions from the provided tokens.
    /// </summary>
    /// <param name="tokens">The array of <see cref="SqlToken"/> representing the assignment expressions.</param>
    /// <returns>An array of <see cref="AssignExpr"/> representing the parsed assignment expressions.</returns>
    private AssignExpr[] ParseAssignExpression(SqlToken[] tokens)
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
            List<AssignExpr> assignments = [];
            int lastPosition = 0;
            for (int i = 0; i < assignmentsCount; i++)
            {
                List<SqlToken> operands = [];
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

    /// <summary>
    /// Parses a condition group from the provided tokens.
    /// </summary>
    /// <param name="conditionTokens">The array of <see cref="SqlToken"/> representing the condition tokens.</param>
    /// <returns>A <see cref="ConditionGroup"/> representing the parsed condition group, or null if no conditions are present.</returns>
    private ConditionGroup? ParseConditionGroup(SqlToken[]? conditionTokens)
    {
        if (conditionTokens is null)
            return null;

        if (conditionTokens.Length == 3)
        {
            return new ConditionGroup(
                subconditions: new [] { new ConditionExpr(conditionTokens) }, 
                logicalOperators: Array.Empty<SqlToken>());
        }

        List<ConditionExpr> conditions = [];
        List<SqlToken> oneConditionTokens = [];
        List<SqlToken> logicalOperators = [];

        for (int i = 0; i < conditionTokens.Length; i++)
        {
            oneConditionTokens.Add(conditionTokens[i]);

            if (conditionTokens[i].IsOperator(OperatorType.Logical))
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

    private ColumnDefExpression[] ParseColumnDefExpressions(SqlToken[] columnDefTokens)
    {
        // Id INTEGER PK AU, Name STRING
        // Id Identifier
        // INTEGER ValueType
        // PK Constraint
        // AU Constraint
        // Name Identifier
        // STRING ValueType
    
        List<ColumnDefExpression> columns = [];
        List<SqlToken> oneExpressionTokens = [];

        for (int i = 0; i < columnDefTokens.Length; i++)
        {
            oneExpressionTokens.Add(columnDefTokens[i]);
            
            if (i == columnDefTokens.Length - 1)
            {
                columns.Add(newColumnDefExpression(oneExpressionTokens));
            }
            else if (columnDefTokens[i + 1].IsType(TokenType.Identifier))
            {
                columns.Add(newColumnDefExpression(oneExpressionTokens));
                oneExpressionTokens.Clear();
            }
        }

        return columns.ToArray();

        ColumnDefExpression newColumnDefExpression(IReadOnlyList<SqlToken> tokens)
        {
            return new ColumnDefExpression(
                columnName: oneExpressionTokens[0],
                valueType: oneExpressionTokens[1],
                constraints: oneExpressionTokens.Count > 2 ? oneExpressionTokens[2..] : null);
        }
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

internal static partial class SqlCoreExtensions
{
    public static string[] Split(this string s, params SeparatorType[] types)
    {
        string[] separators = types.Select(EnumsStorage.GetSeparatorString).ToArray();
        return s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    public static bool IsNumeric(this char c) => "0123456789.".Contains(c);
    public static bool IsNumeric(this string s) => s.All(IsNumeric);

    public static int IndexOf(this SqlToken[] tokens, SqlToken? token) => Array.IndexOf(tokens, token);

    public static int IndexOf(this SqlToken[] tokens, Keyword kw) =>
        tokens.IndexOf(tokens.FirstOrDefault(t => t.IsKeyword(kw)));
    
    public static string[] TokenValuesToArray(this SqlToken[] tokens) => tokens.Select(t => t.Value).ToArray(); 
}