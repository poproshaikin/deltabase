using Data.Definitions;
using Data.Definitions.Schemes;
using Enums.Exceptions;
using Enums.Records.Columns;
using Enums.Sql.Tokens;
using Sql.Shared.Expressions;
using Sql.Shared.Queries;
using Sql.Shared.Tokens;
using Utils;

namespace Sql.Executing.App;

public class QueryValidator
{
    private const int MaxTableNameLength = 20;
    private const string RestrictedSymbols = "\\/:*?\"<>|%{}^~[]'";

    private const int MinLength_Select = 4; // SELECT * FROM <tableName>
    private const int MinLength_CreateTable = 5; // CREATE TABLE <name> <columnName> <valueType>

    private DataDefinitor _dataDef;

    public QueryValidator(string dbName, FileSystemHelper fs)
    {
        _dataDef = new DataDefinitor(dbName, fs);
    }
    
    public ValidationResult Validate(ISqlQuery parsedQuery)
    {
        return parsedQuery switch
        {
            SelectQuery select => ValidateSelect(select),
            InsertQuery insert => ValidateInsert(insert),
            UpdateQuery update => ValidateUpdate(update),
            DeleteQuery delete => ValidateDelete(delete),

            _ => throw new NotImplementedException()
        };
    }

    private ValidationResult ValidateSelect(SelectQuery query)
    {
        SelectExpr select = query.Select;

        if (select is { AllColumns: false, ColumnNames: null })
                return new ValidationResult(false, query, ErrorType.InvalidPassedColumns);
            
        if (!ValidateFromExpr(query.From, out ErrorType error))
                return new ValidationResult(false, query, ErrorType.TableDoesntExist);

        if (query.Condition is not null)
                if (!ValidateConditionGroup(query.Condition, query.From.TableName, out error))
                    return new ValidationResult(false, query, error);

        return new ValidationResult(true, query, default);
    }

    private ValidationResult ValidateInsert(InsertQuery query)
    {
        InsertExpr insert = query.Insert;

        // if table doesnt exist
        if (!_dataDef.TableExists(insert.TableName))
                return new ValidationResult(false, query, ErrorType.TableDoesntExist);
        
        string[] columnNames = query.Insert.ColumnNames;
        TableScheme table = _dataDef.GetTableScheme(query.Insert.TableName);
            
        // if some column doesnt exist
        if (columnNames.Any(columnName => !table.HasColumn(columnName)))
            return new ValidationResult(false, query, ErrorType.ColumnDoesntExist);
        
        // if the count of a passed values not equals to columns passed
        if (query.Values.Any(v => v.Values.Length != columnNames.Length))
            // TODO сделать автозаполнение колонок поддерживающих null
            // if some of the columns wasn't passed, except of primary key 
            if (table.PrimaryKey is not null && query.Insert.ColumnNames.Any(c => c == table.PrimaryKey.Name))
                return new ValidationResult(false, query, ErrorType.InvalidPassedColumns);
        
        return new ValidationResult(true, query, default);
    }

    private ValidationResult ValidateUpdate(UpdateQuery query)
    {
        UpdateExpr update = query.Update;
            
        if (!_dataDef.TableExists(update.TableName))
                return new ValidationResult(false, query, ErrorType.TableDoesntExist);
        
        AssignExpr[] assignments = query.Set.Assignments;
        foreach (AssignExpr assignment in assignments)
                if (!ValidateAssignment(assignment, query.Update.TableName, out ErrorType error))
                    return new ValidationResult(false, query, error);
        
        if (query.Condition is not null)
                if (!ValidateConditionGroup(query.Condition, query.Update.TableName, out ErrorType error))
                    return new ValidationResult(false, query, error);

        return new ValidationResult(true, query, default);
    }

    private ValidationResult ValidateDelete(DeleteQuery query)
    {
        if (!ValidateFromExpr(query.From, out ErrorType error))
            return new ValidationResult(false, query, error);
        
        if (query.Condition is not null)
            if (!ValidateConditionGroup(query.Condition, query.From.TableName, out error))
                return new ValidationResult(false, query, error);
        
        return new ValidationResult(true, query, default);
    }

    private bool ValidateFromExpr(FromExpr from, out ErrorType error)
    {
        if (!_dataDef.TableExists(from.TableName))
        {
            error = ErrorType.TableDoesntExist;
            return false;
        }

        error = default;
        return true;
    }

    private bool ValidateConditionGroup(ConditionGroup conditionGroup, string tableName, out ErrorType error)
    {
        for (var i = 0; i < conditionGroup.Subconditions.Length; i++)
        {
            ConditionExpr condition = conditionGroup.Subconditions[i];
            if (!ValidateCondition(condition, tableName, out error))
            {
                return false;
            }

            if (i != (conditionGroup.Subconditions.Length - 1))
            {
                SqlToken @operator = conditionGroup.LogicalOperators[i];

                if (!@operator.IsOperator(OperatorType.And) || !@operator.IsOperator(OperatorType.Or))
                {
                    error = ErrorType.InvalidCondition;
                    return false;
                }
            }
        }
        
        error = default;
        return true;
    }

    private bool ValidateCondition(ConditionExpr condition, string tableName, out ErrorType error)
    {
        TableScheme table = _dataDef.GetTableScheme(tableName);
            
        SqlToken left = condition.LeftOperand;
        SqlToken right = condition.RightOperand;

        if (!left.IsType(TokenType.Identifier))
        {
            error = ErrorType.InvalidPassedValues;
            return false;
        }

        ColumnScheme leftCol = table.GetColumn(left)!;

        if (right.IsType(TokenType.Identifier))
        {
            ColumnScheme rightCol = table.GetColumn(right)!;

            if (leftCol.ValueType != rightCol.ValueType)
            {
                error = ErrorType.InvalidPassedValues;
                return false;
            }
        }
        else
        {
            if (!CheckTypeMatch(leftCol.ValueType, right))
            {
                error = ErrorType.InvalidValueType;
                return false;
            }
        }
        
        error = default;
        return true;
    }

    private bool ValidateAssignment(AssignExpr assignment, string tableName, out ErrorType error)
    {
        if (!assignment.Operator.IsOperator(OperatorType.Assign))
        {
            error = ErrorType.InvalidAssignment;
            return false;
        }

        TableScheme scheme = _dataDef.GetTableScheme(tableName);
        
        SqlToken left = assignment.LeftOperand;
        SqlToken right = assignment.RightOperand;

        ColumnScheme leftCol = scheme.GetColumn(left)!;
        ColumnScheme rightCol = scheme.GetColumn(right)!;

        if (!left.IsType(TokenType.Identifier))
        {
            error = ErrorType.InvalidAssignment;
            return false;
        }
        else if (!scheme.HasColumn(left))
        {
            error = ErrorType.ColumnDoesntExist;
            return false;
        }

        if (right.IsType(TokenType.Identifier))
        {
            if (!scheme.HasColumn(right))
            {
                error = ErrorType.ColumnDoesntExist;
                return false;
            }

            if (leftCol.ValueType != rightCol.ValueType)
            {
                if (leftCol.ValueType != SqlValueType.Integer || rightCol.ValueType != SqlValueType.Float)
                {
                    error = ErrorType.InvalidValueType;
                    return false;
                }

                if (leftCol.ValueType != SqlValueType.String || rightCol.ValueType != SqlValueType.Char)
                {
                    error = ErrorType.InvalidValueType;
                    return false;
                }
            }
        }
        else
        {
            if (!CheckTypeMatch(leftCol.ValueType, right))
            {
                error = ErrorType.InvalidValueType;
                return false;
            }
        }

        error = default;
        return true;
    }

    private bool CheckTypeMatch(SqlValueType valueType, string value)
    {
        return valueType switch
        {
            SqlValueType.Integer when !int.TryParse(value, out _) => false,
            SqlValueType.Float when !float.TryParse(value, out _) => false,
            SqlValueType.Char when value.Length != 1 => false,
            _ => true
        };
    }
}