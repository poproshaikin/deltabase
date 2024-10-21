using Db.DataStorage;
using Enums.Exceptions;
using Enums.Sql.Tokens;
using Enums.Tcp;
using Sql.Core;
using Sql.Expressions;
using Sql.Queries;
using Sql.Tokens;

namespace Db.Core;

public class QueryValidator
{
    private const int MaxTableNameLength = 20;
    private const string RestrictedNameSymbols = "\\/:*?\"<>|%{}^~[]'";

    private const int MinLength_Select = 4; // SELECT * FROM <tableName>
    private const int MinLength_CreateTable = 5; // CREATE TABLE <name> <columnName> <valueType>

    private RecordsManager _recordsManager;

    public QueryValidator(RecordsManager recordsManager)
    {
        _recordsManager = recordsManager;
    }
    
    public ValidationResult Validate(IParsedQuery parsedQuery)
    {
        return parsedQuery switch
        {
            SelectQuery select => ValidateSelect(select),

            _ => throw new NotImplementedException()
        };
    }
}