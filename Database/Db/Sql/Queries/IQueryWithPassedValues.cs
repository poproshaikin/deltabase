using Sql.Tokens;

namespace Sql.Queries;

public interface IQueryWithPassedValues
{
    SqlToken[] PassedValues { get; set; }
}