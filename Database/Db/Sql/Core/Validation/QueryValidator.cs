using Enums.Sql.Tokens;
using Enums.Tcp;
using Sql.Queries;
using Sql.Tokens;

namespace Sql.Core.Validation;

/// <summary>
/// Represents a validator for SQL queries.
/// </summary>
public class QueryValidator
{
    private const int MaxTableNameLength = 20;
    private const string RestrictedNameSymbols = "\\/:*?\"<>|%{}^~[]'";

    private const int MinLength_Select = 4; // SELECT * FROM <tableName>
    private const int MinLength_CreateTable = 5; // CREATE TABLE <name> <columnName> <valueType>
    
    /// <summary>
    /// Validates the provided SQL query.
    /// </summary>
    /// <param name="query">The SQL query to validate.</param>
    /// <param name="errorCode">The error code returned if the query is invalid.</param>
    /// <returns>True if the query is invalid; otherwise, false.</returns>
    /// <exception cref="NotImplementedException">Thrown when the query type is not supported.</exception>
    public bool IsInvalid(SqlQuery query, out ResponseType errorCode)
    {
        return query switch
        {
            SelectQuery select => IsSelectInvalid(select, out errorCode),
            CreateTableQuery create => IsCreateInvalid(create, out errorCode),
            
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Validates a SELECT query.
    /// </summary>
    /// <param name="select">The SELECT query to validate.</param>
    /// <param name="errorCode">The error code returned if the SELECT query is invalid.</param>
    /// <returns>True if the SELECT query is invalid; otherwise, false.</returns>
    private bool IsSelectInvalid(SelectQuery select, out ResponseType errorCode)
    {
        SelectValidationResult validation = new();

        SqlToken[] tokens = select.Tokens;

        if (tokens.Length is < MinLength_Select)
        {
            errorCode = ResponseType.InvalidQueryLength;
            return true;
        }

        int selectIndex = select.IndexOf(Keyword.Select);
        int fromIndex = select.IndexOf(Keyword.From);
        
        if (fromIndex == -1 || selectIndex != 0)
        {
            errorCode = ResponseType.InvalidSql;
            return true;
        }
        
        validation.IsSqlValid = true;
        
        int whereIndex = select.IndexOf(Keyword.Where);
        if (whereIndex != -1)
        {
            SqlToken[] conditionTokens = tokens[(whereIndex + 1)..];
            int expressionsCount = conditionTokens.Count(t => !t.IsOperator(OperatorType.Logical)) / 3;
            
            if (conditionTokens.Length != 4 * expressionsCount - 1) // the function, describing the tokens count we can define as  4 * n - 1  
            {                                                       // where n is the number of expressions
                errorCode = ResponseType.InvalidQueryLength;
                return true;
            }
        }

        errorCode = default;
        return validation.IsValid;
    }

    /// <summary>
    /// Validates a CREATE TABLE query.
    /// </summary>
    /// <param name="create">The CREATE TABLE query to validate.</param>
    /// <param name="errorCode">The error code returned if the CREATE TABLE query is invalid.</param>
    /// <returns>True if the CREATE TABLE query is invalid; otherwise, false.</returns>
    private bool IsCreateInvalid(CreateTableQuery create, out ResponseType errorCode)
    {
        CreateValidationResult validation = new();

        string tableName = create.TableName;
        
        if (tableName.Length is 0 or > MaxTableNameLength)
        {
            errorCode = ResponseType.InvalidTableNameLength;
            return true;
        }

        validation.IsNameLengthValid = true;

        if (tableName.Select(c => RestrictedNameSymbols.Contains(c)).Any())
        {
            errorCode = ResponseType.InvalidTableName;
            return true;
        }

        validation.IsNameValid = true;
        SqlToken[] tokens = create.Tokens;

        if (tokens.Length < MinLength_CreateTable)
        {
            errorCode = ResponseType.InvalidQueryLength;
            return true;
        }

        if (!tokens[0].IsKeyword(Keyword.Create) &&
            !tokens[1].IsKeyword(Keyword.Table) &&
            !tokens[2].IsType(TokenType.Identifier) &&
            !tokens[3].IsType(TokenType.Identifier) &&
            !tokens[4].IsType(TokenType.ValueType))
        {
            errorCode = ResponseType.InvalidSql;
            return true;
        }

        validation.IsSqlValid = true;

        errorCode = default;
        return validation.IsValid;
    }
}