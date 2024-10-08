using Sql.Tokens;

namespace Sql.Common.Queries;

public interface IQueryWithPassedValues
{
    SqlToken[] PassedValues { get; set; }
}