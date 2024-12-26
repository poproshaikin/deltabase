using Data.Definitions.Schemes;
using Data.Models;
using Enums;
using Enums.Exceptions;
using Enums.Sql.Tokens;
using Exceptions;
using Sql.Shared.Expressions;
using Sql.Shared.Tokens;
using Utils;

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

    public bool Check(PageRow pageRow)
    {
        bool[] results = (from subcondition in Condition.Subconditions
            let left = ParseOperand(pageRow,
                subcondition.LeftOperand)
            let right = ParseOperand(pageRow,
                subcondition.RightOperand)
            select CompareValues(left,
                right,
                subcondition.Operator)).ToArray();

        SqlToken[] logicalOperators = Condition.LogicalOperators;
        if (logicalOperators.Length == 0)
        {
            if (results.Length == 1)
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
    
    private object ParseOperand(PageRow pageRow, SqlToken operand)
    {
        if (operand.IsType(TokenType.Identifier))
        {
            int columnId = SourceTableScheme.IndexOfColumn(operand);
            return pageRow.Data[columnId];
        }
        else
        {
            return operand;
        }
    }
    
    private bool CompareValues(object left, object right, SqlToken @operator)
    {
        return EnumsStorage.GetOperatorType(@operator) switch
        {
            OperatorType.Equals => OperandsEquals(left, right),
            OperatorType.LessThan => Convert.ToSingle(left) < Convert.ToSingle(right),
            OperatorType.LessThanOrEquals => Convert.ToSingle(left) <= Convert.ToSingle(right),
            OperatorType.GreaterThan => Convert.ToSingle(left) > Convert.ToSingle(right),
            OperatorType.GreaterThanOrEquals => Convert.ToSingle(left) >= Convert.ToSingle(right),
            OperatorType.NotEquals => left != right,

            _ => throw new DbEngineException(ErrorType.InvalidLogicalOperatorInCondition)
        };
    }

    private bool OperandsEquals(object left, object right)
    {
        if (left is string leftS && right is string rightS)
        {
            return leftS == rightS;
        }

        return Math.Abs(Convert.ToSingle(left) - Convert.ToSingle(right)) < 0.00001f;
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