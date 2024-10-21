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
    private SqlTokenizer _tokenizer;

    public QueryParser()
    {
        _tokenizer = new SqlTokenizer();
    }
    
    /// <summary>
    /// Parses the provided SQL string and converts it into a <see cref="SqlQuery"/> object.
    /// </summary>
    /// <param name="rawQuery">The SQL query string to parse.</param>
    /// <returns>A <see cref="SqlQuery"/> object representing the parsed SQL.</returns>
    public IParsedQuery Parse(string rawQuery)
    {
        SqlToken[] tokens = _tokenizer.Tokenize(rawQuery);

        return ToQuery(tokens);
    }

    /// <summary>
    /// Converts an array of SQL tokens into the corresponding <see cref="SqlQuery"/> object.
    /// </summary>
    /// <param name="tokens">The array of <see cref="SqlToken"/> to convert.</param>
    /// <returns>A <see cref="SqlQuery"/> object representing the parsed tokens.</returns>
    private IParsedQuery ToQuery(SqlToken[] tokens)
    {
        return tokens[0] switch
        {
            { } t when t.IsKeyword(Keyword.Select) => ParseSelect(tokens),
    
            _ => throw new NotImplementedException()
        };
    }

    private SelectQuery ParseSelect(SqlToken[] tokens)
    {
        List<SqlExpression> expressions = [];
        
        int 
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