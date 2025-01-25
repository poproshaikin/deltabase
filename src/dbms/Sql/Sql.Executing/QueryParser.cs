using Enums;
using Enums.Exceptions;
using Enums.Sql.Tokens;
using Exceptions;
using Sql.Shared.Expressions;
using Sql.Shared.Queries;
using Sql.Shared.Tokens;

namespace Sql.Executing;

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
    /// Parses the provided SQL string and converts it into a <see cref="ISqlQuery"/> object.
    /// </summary>
    /// <param name="rawQuery">The SQL query string to parse.</param>
    /// <returns>A <see cref="ISqlQuery"/> object representing the parsed SQL.</returns>
    public ISqlQuery Parse(string rawQuery)
    {
        SqlToken[] tokens = _tokenizer.Tokenize(rawQuery);
        SqlExpr[] expressions = ToExpressions(tokens);

        return ToQuery(expressions);
    }

    /// <summary>
    /// Converts an array of SQL tokens into the corresponding <see cref="ISqlQuery"/> object.
    /// </summary>
    /// <param name="tokens">The array of <see cref="SqlToken"/> to convert.</param>
    /// <returns>A <see cref="ISqlQuery"/> object representing the parsed tokens.</returns>
    private ISqlQuery ToQuery(SqlExpr[] expressions)
    {
        return expressions[0] switch
        {
            SelectExpr => ParseSelect(expressions),
            InsertExpr => ParseInsert(expressions),
            UpdateExpr => ParseUpdate(expressions),
            DeleteExpr => ParseDelete(expressions),
    
            _ => throw new NotImplementedException()
        };
    }

    private SelectQuery ParseSelect(SqlExpr[] exprs)
    {
        SelectQuery query = new();

        foreach (SqlExpr expression in exprs)
        {
            switch (expression)
            {
                case SelectExpr select:
                    query.Select = select;
                    break;
                case FromExpr from:
                    query.From = from;
                    break;
                case ConditionGroup condition:
                    query.Condition = condition;
                    break;
                case LimitExpr limit: 
                    query.Limit = limit;
                    break;
                default:
                    throw new DbEngineException(ErrorType.InvalidQuery);
            }
        }

        return query;
    }

    private InsertQuery ParseInsert(SqlExpr[] expressions)
    {
        InsertQuery query = new();

        foreach (SqlExpr expression in expressions)
        {
            switch (expression)
            {
                case InsertExpr insert:
                    query.Insert = insert;
                    break;
                case ValuesExpr values:
                    query.Values = query.Values.Append(values).ToArray();
                    break;
                default:
                    throw new DbEngineException(ErrorType.InvalidQuery);
            }
        }

        return query;
    }

    private UpdateQuery ParseUpdate(SqlExpr[] expressions)
    {
        UpdateQuery query = new();

        foreach (SqlExpr expression in expressions)
        {
            switch (expression)
            {
                case UpdateExpr update:
                    query.Update = update;
                    break;
                case SetExpr set:
                    query.Set = set;
                    break;
                case ConditionGroup condition: 
                    query.Condition = condition;
                    break;
                default:
                    throw new DbEngineException(ErrorType.InvalidQuery);
            }
        }

        return query;
    }

    private DeleteQuery ParseDelete(SqlExpr[] expressions)
    {
        DeleteQuery delete = new();

        foreach (SqlExpr expression in expressions)
        {
            switch (expression)
            {
                case FromExpr from:
                    delete.From = from;
                    break;
                case ConditionGroup condition:
                    delete.Condition = condition;
                    break;
                case LimitExpr limit:
                    delete.Limit = limit;
                    break;
                default:
                    throw new DbEngineException(ErrorType.InvalidQuery);
            }
        }

        return delete;
    }

    private SqlExpr[] ToExpressions(SqlToken[] tokens)
    {
        List<SqlExpr> expressions = [];

        for (int i = 0; i < tokens.Length; i++)
        {
            #region Global
            
            if (tokens[i].IsKeyword(Keyword.Where))
            {
                i++;
                
                int lastIndex = tokens.FirstKeywordAfter(i);

                SqlToken[] logicalExpressionTokens = tokens[i..lastIndex];
                if (logicalExpressionTokens.Length == 0)
                    throw new DbEngineException(ErrorType.InvalidCondition);
                
                expressions.Add(ParseConditionGroup(logicalExpressionTokens));

                i += logicalExpressionTokens.Length - 1;
            }
            else if (tokens[i].IsKeyword(Keyword.From))
            {
                i++;
                
                expressions.Add(new FromExpr()
                {
                    TableName = tokens[i]
                });

                continue;
            }
            
            #endregion
            
            #region Select
            
            else if (tokens[i].IsKeyword(Keyword.Select))
            {
                int fromIndex = tokens.IndexOf(Keyword.From);
                SqlToken[] columnNames = tokens[(i + 1)..fromIndex];
 
                if (columnNames.Length == 0)
                {
                    throw new DbEngineException(ErrorType.MissingPassedColumns);
                }
                
                if (columnNames.Length == 1 && columnNames[0].IsOperator(OperatorType.Asterisk))
                {
                    expressions.Add(new SelectExpr()
                    {
                        AllColumns = true,
                        ColumnNames = null
                    });
                }
                else
                {
                    expressions.Add(new SelectExpr()
                    {
                        AllColumns = false,
                        ColumnNames = columnNames.ValuesToArray()
                    });
                }

                i += columnNames.Length;
            }
            else if (tokens[i].IsKeyword(Keyword.Limit))
            {
                i++;
                
                expressions.Add(new LimitExpr()
                {
                    Limit = int.Parse(tokens[i])
                });

                continue;
            }
            #endregion
            
            #region Insert 
            
            else if (tokens[i].IsKeyword(Keyword.Insert))
            {
                i += 2; // INSERT INTO <here>

                string tableName = tokens[i];

                int lastIndex = tokens.FirstKeywordAfter(i);

                i++;

                string[] columnNames = tokens[i..lastIndex].ValuesToArray();

                if (columnNames.Length == 0)
                {
                    throw new DbEngineException(ErrorType.InvalidPassedColumns);
                }
                    
                expressions.Add(new InsertExpr()
                {
                    TableName = tableName,
                    ColumnNames = columnNames
                });

                continue;
            }
            else if (tokens[i].IsKeyword(Keyword.Values))
            {
                i++;

                int lastIndex = tokens.FirstKeywordAfter(i);

                string[] textValues = tokens[i..lastIndex].ValuesToArray();

                if (textValues.Length == 0)
                {
                    throw new DbEngineException(ErrorType.InvalidPassedValues);
                }
                
                expressions.Add(new ValuesExpr()
                {
                    Values = textValues
                });
                
                continue;
            }
            
            #endregion
            
            #region Update
            
            else if (tokens[i].IsKeyword(Keyword.Update))
            {
                i++;
                
                expressions.Add(new UpdateExpr()
                {
                    TableName = tokens[i]
                });

                continue;
            }
            else if (tokens[i].IsKeyword(Keyword.Set))
            {
                i++;

                int whereIndex = tokens.IndexOf(Keyword.Where);

                AssignExpr[] assignment = ParseAssignExpression(
                    whereIndex == -1 ? tokens[i..] : tokens[i..whereIndex]);
                
                expressions.Add(new SetExpr(assignment));

                continue;
            }
            
            #endregion
            
            #region Delete 
            
            else if (tokens[i].IsKeyword(Keyword.Delete))
            {
                throw new NotImplementedException();
            }
            
            #endregion
        }
        
        return expressions.ToArray();
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
            return
            [
                new AssignExpr()
                {
                    LeftOperand = tokens[0],
                    RightOperand = tokens[^1],
                }
            ];
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
                assignments.Add(new AssignExpr()
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
    private ConditionGroup ParseConditionGroup(SqlToken[] conditionTokens)
    {
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

    public static int FirstKeywordAfter(this SqlToken[] tokens, int startIndex) => 
        tokens.IndexOf(tokens[startIndex..].FirstOrDefault(t => t.IsType(TokenType.Keyword)));

    public static string[] ValuesToArray(this SqlToken[] tokens) => tokens.Select(t => t.Value).ToArray(); 
}