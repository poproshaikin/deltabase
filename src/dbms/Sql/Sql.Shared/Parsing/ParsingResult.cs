using Enums.Exceptions;
using Sql.Shared.Queries;

namespace Sql.Shared.Parsing;

public class ParsingResult
{
    public bool IsValid { get; set; }
    public ErrorType Error { get; set; }
    public ISqlQuery ParsedAndValidatedQuery { get; set; }
}