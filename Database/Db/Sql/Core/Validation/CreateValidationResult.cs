using System.Diagnostics.CodeAnalysis;

namespace Sql.Core.Validation;

public class CreateValidationResult : IValidationResult
{
     public bool IsValid =>
          IsNameLengthValid &&
          IsNameValid && 
          IsSqlValid;
     
     public bool IsNameLengthValid { get; set; }
     public bool IsNameValid { get; set; }
     public bool IsSqlValid { get; set; }
}