using Enums;
using Enums.Exceptions;
using Enums.Sql.Tokens;
using Exceptions;
using Sql.Expressions;
using Sql.Tokens;

namespace Db.DataStorage;

/// <summary>
/// Class for checking conditions based on the provided data.
/// </summary>
public class ConditionChecker
{
    public TableDataSet SourceTableData { get; private set; }
    public ConditionGroup Condition { get; private set; }

    public void SetTargetTable(TableDataSet table)
    {
        SourceTableData = table;
    }

    public void SetCondition(ConditionGroup condition)
    {
        Condition = condition;
    }

    public bool Check(RowDataSet row)
    {
        List<bool> results = [];

        foreach (ConditionExpr subcondition in Condition.Subconditions)
        {
            string left = ParseOperand(row, subcondition.LeftOperand);
            string right = ParseOperand(row, subcondition.RightOperand);
            
            bool expressionResult = CompareValues(left, right, subcondition.Operator);
            results.Add(expressionResult);
        }
        
        SqlToken[] logicalOperators = Condition.LogicalOperators;
        if (logicalOperators.Length == 0)
        {
            if (results.Count == 1)
            {
                return results[0];
            }
            else
            {
                throw new DbEngineException(ErrorType.InvalidCondition);
            }
        }

        return EvaluateLogicalExpression(results, logicalOperators);
    }
    
    private string ParseOperand(RowDataSet row, SqlToken operand)
    {
        if (operand.IsType(TokenType.Identifier))
        {
            int columnId = SourceTableData.IndexOfColumn(operand);
            return row[columnId];
        }
        else
        {
            return operand;
        }
    }
    
    private bool CompareValues(string left, string right, SqlToken @operator)
    {
        return EnumsStorage.GetOperatorType(@operator) switch
        {
            OperatorType.Equals => left == right,
            OperatorType.LessThan => int.Parse(left) < int.Parse(right),
            OperatorType.LessThanOrEquals => int.Parse(left) <= int.Parse(right),
            OperatorType.GreaterThan => int.Parse(left) > int.Parse(right),
            OperatorType.GreaterThanOrEquals => int.Parse(left) >= int.Parse(right),
            OperatorType.NotEquals => left != right,

            _ => throw new InvalidOperationException("Tried to compare with an invalid operator")
        };
    }
    private bool EvaluateLogicalExpression(IReadOnlyList<bool> results, IReadOnlyList<SqlToken> logicalOperators)
    {
        bool result = false;
         
        for (int i = 0; i < logicalOperators.Count; i++)
        {
            if (i == 0) 
                result = results[i] && results[i + 1];

            result = EnumsStorage.GetOperatorType(logicalOperators[i]) switch
            {
                OperatorType.And => result && results[i + 1],
                OperatorType.Or => result || results[i + 1],
                 
                _ => throw new NotImplementedException()
            };
        }

        return result; 
    }
}

// private Record _sourceRecord;
//     private ConditionGroup _condition;
//
//     /// <summary>
//     /// Initializes a new instance of the <see cref="ConditionChecker"/> class to evaluate conditions.
//     /// </summary>
//     /// <param name="sourceRecord">The source record containing columns and their values.</param>
//     /// <param name="condition">The group of conditions to be checked.</param>
//     /// <exception cref="InvalidSqlException">Thrown if the provided condition is invalid.</exception>
//     public ConditionChecker(Record sourceRecord, ConditionGroup condition) // "Id" == 6
//     {
//         _sourceRecord = sourceRecord;
//         
//         ThrowIfInvalid(condition);
//         _condition = condition;
//     }
//
//     /// <summary>
//     /// Checks whether the conditions are met for the given data row.
//     /// </summary>
//     /// <param name="row">The data row to evaluate the conditions against.</param>
//     /// <returns>
//     /// <c>true</c> if the conditions are satisfied; otherwise, <c>false</c>.
//     /// </returns>
//     /// <exception cref="InvalidSqlException">Thrown if the SQL expression is invalid.</exception>
//     public bool IsMet(RecordRow row) // Id == 6 AND Name == "Ivan"
//     {
//         List<bool> results = [];
//
//         foreach (ConditionExpr expr in _condition.Subconditions)
//         {
//             string left = ParseOperand(row, expr.LeftOperand);
//             string right = ParseOperand(row, expr.RightOperand);
//
//             bool expressionResult = CompareValues(left, right, expr.Operator);
//             results.Add(expressionResult);
//         }
//
//         SqlToken[] logicalOperators = _condition.LogicalOperators;
//
//         if (logicalOperators.Length == 0)
//         {
//             if (results.Count == 1)
//             {
//                 return results[0];
//             }
//             else
//             {
//                 throw ThrowHelper.InvalidSql();
//             }
//         }
//
//         return EvaluateLogicalExpression(results, logicalOperators);
//     }
//
//     /// <summary>
//     /// Parses an operand and returns its value based on the row data.
//     /// </summary>
//     /// <param name="row">The data row containing values.</param>
//     /// <param name="operand">The SQL token representing the operand.</param>
//     /// <returns>The value of the operand.</returns>
//     private string ParseOperand(RecordRow row, SqlToken operand)
//     {
//         if (operand.IsType(TokenType.Identifier))
//         {
//             int columnId = _sourceRecord.GetColumnId(operand);
//             return row[columnId];
//         }
//         else
//         {
//             return operand;
//         }
//     }
//
//     /// <summary>
//     /// Compares two values based on the given operator.
//     /// </summary>
//     /// <param name="left">The left-hand value.</param>
//     /// <param name="right">The right-hand value.</param>
//     /// <param name="operator">The operator used for comparison.</param>
//     /// <returns><c>true</c> if the comparison is true; otherwise, <c>false</c>.</returns>
//     private bool CompareValues(string left, string right, SqlToken @operator)
//     {
//         return EnumsStorage.GetOperatorType(@operator) switch
//         {
//             OperatorType.Equals => left == right,
//             OperatorType.LessThan => int.Parse(left) < int.Parse(right),
//             OperatorType.LessThanOrEquals => int.Parse(left) <= int.Parse(right),
//             OperatorType.GreaterThan => int.Parse(left) > int.Parse(right),
//             OperatorType.GreaterThanOrEquals => int.Parse(left) >= int.Parse(right),
//             OperatorType.NotEquals => left != right,
//                 
//             _ => throw new InvalidOperationException("Tried to compare with an invalid operator")
//         };
//     }
//
//     /// <summary>
//     /// Evaluates the result of a logical expression based on the provided results and logical operators.
//     /// </summary>
//     /// <param name="results">A list of boolean results from individual condition checks.</param>
//     /// <param name="logicalOperators">A list of logical operators to combine the results.</param>
//     /// <returns><c>true</c> if the logical expression evaluates to true; otherwise, <c>false</c>.</returns>
//     /// <exception cref="NotImplementedException">Thrown if an unsupported logical operator is encountered.</exception>
//     private bool EvaluateLogicalExpression(IReadOnlyList<bool> results, IReadOnlyList<SqlToken> logicalOperators)
//     {
//         bool result = false;
//         
//         for (int i = 0; i < logicalOperators.Count; i++)
//         {
//             if (i == 0) 
//                 result = results[i] && results[i + 1];
//
//             result = EnumsStorage.GetOperatorType(logicalOperators[i]) switch
//             {
//                 OperatorType.And => result && results[i + 1],
//                 OperatorType.Or => result || results[i + 1],
//                 
//                 _ => throw new NotImplementedException()
//             };
//         }
//
//         return result;
//     }
//
//     /// <summary>
//     /// Throws an exception if the provided condition group is invalid.
//     /// </summary>
//     /// <param name="condition">The condition group to validate.</param>
//     /// <exception cref="InvalidSqlException">Thrown if any column in the condition is invalid or unexpected.</exception>
//     private void ThrowIfInvalid(ConditionGroup condition)
//     {
//         SqlToken[] operands = condition.GetLeftOperands().Concat(condition.GetRightOperands()).ToArray();
//
//         foreach (SqlToken operand in operands)
//             if (operand.IsType(TokenType.Identifier))
//                 if (!_sourceRecord.ContainsColumn(operand))
//                     throw new InvalidOperationException(
//                         $"Passed column {operand} in the table {_sourceRecord.Name} does not exist.");
//     }