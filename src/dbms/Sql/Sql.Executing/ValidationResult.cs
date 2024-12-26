using Enums.Exceptions;
using Sql.Shared.Queries;

namespace Sql.Executing.App;

public class ValidationResult
{
    public bool IsValid;
    public ISqlQuery Query;
    public ErrorType Error;
    
    public ValidationResult(bool isValid, ISqlQuery query, ErrorType error)
    {
        IsValid = isValid;
        Query = query;
        Error = error;
    }
}