using Enums.Exceptions;
using Enums.Tcp;
using Sql.Queries;

namespace Sql.Core;

public class ValidationResult
{
    public bool IsValid;
    public IValidatedQuery Query;
    public ErrorType Error;
    
    public ValidationResult(bool isValid, IValidatedQuery query, ErrorType error)
    {
        IsValid = isValid;
        Query = query;
        Error = error;
    }
}