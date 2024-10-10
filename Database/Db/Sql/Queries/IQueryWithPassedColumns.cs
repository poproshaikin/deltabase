using Sql.Tokens;

namespace Sql.Queries;

public interface IQueryWithPassedColumns
{
    SqlToken[] PassedColumns { get; set; }
}