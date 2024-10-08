using Db.Records;
using Enums;
using Enums.Sql.Tokens;
using Sql.Expressions;
using Sql.Tokens;
using Utils;

namespace Sql.Core;

public class ConditionChecker
{
    private Record _sourceRecord;
    private ConditionGroup _condition;

    public ConditionChecker(Record sourceRecord, ConditionGroup condition) // "Id" == 6
    {
        _sourceRecord = sourceRecord;
        
        ThrowIfInvalid(condition);
        _condition = condition;
    }

    private void ThrowIfInvalid(ConditionGroup condition)
    {
        SqlToken[] leftOperands = condition.GetLeftOperands();

        foreach (SqlToken columnName in leftOperands)
        {
            if (_sourceRecord.Columns.All(c => c.Name != columnName))
            {
                ThrowHelper.ThrowInvalidSql();
            }
            else if (columnName.IsLiteral())
            {
                ThrowHelper.ThrowUnexpectedToken(columnName);
            }
        }
    }

    public bool IsMet(RecordRow row) // Id == 6 AND Name == "Ivan"
    {
        List<bool> results = new();

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (ConditionExpr expr in _condition.Subconditions)
        {
            int columnId = _sourceRecord.GetColumnId(expr.LeftOperand);
            string value = row.Values[columnId];

            results.Add(value == expr.RightOperand);
        }

        SqlToken[] operators = _condition.LogicalOperators;

        if (operators.Length == 0)
        {
            if (results.Count == 1)
            {
                return results[0];
            }
            else
            {
                throw ThrowHelper.InvalidSql();
            }
        }

        bool result = false;
        
        for (int i = 0; i < operators.Length; i++)
        {
            if (i == 0) 
                result = results[i] && results[i + 1];

            result = EnumsStorage.GetOperatorType(operators[i]) switch
            {
                OperatorType.And => result && results[i + 1],
                OperatorType.Or => result || results[i + 1],
                
                _ => throw new NotImplementedException()
            };
        }

        return result;
    }
}