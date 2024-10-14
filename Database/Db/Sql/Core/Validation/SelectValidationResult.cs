namespace Sql.Core.Validation;

public class SelectValidationResult : IValidationResult
{
    public bool IsValid { get; }
    
    public bool IsSqlValid { get; set; }
}