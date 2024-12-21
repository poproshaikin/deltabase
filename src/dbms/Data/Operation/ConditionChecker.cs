using Data.Definitions.Schemes;
using Data.Models;
using Enums;
using Enums.Exceptions;
using Enums.Sql.Tokens;
using Exceptions;
using Sql.Shared.Expressions;
using Sql.Shared.Tokens;

namespace Data.Operation;

public class ConditionChecker
{
    public TableScheme SourceTableScheme { get; private set; } 
    
    public ConditionGroup Condition { get; private set; }

    public ConditionChecker(TableScheme table, ConditionGroup condition)
    {
        SourceTableScheme = table;
        Condition = condition;
    }

    public bool Check(RowModel row)
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
    
    private string ParseOperand(RowModel row, SqlToken operand)
    {
        if (operand.IsType(TokenType.Identifier))
        {
            int columnId = SourceTableScheme.IndexOfColumn(operand);
            return row.Data[columnId];
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

            _ => throw new DbEngineException(ErrorType.InvalidLogicalOperatorInCondition)
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
                 
                _ => throw new DbEngineException(ErrorType.InvalidLogicalOperatorInCondition)
            };
        }

        return result; 
    }
}

file static class Extensions
{
    internal static int IndexOfColumn(this TableScheme table, string columnName)
    {
        return Array.IndexOf(table.Columns.Select(c => c.Name).ToArray(), columnName);
    }
}