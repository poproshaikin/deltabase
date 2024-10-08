using Sql.Tokens;

namespace Sql.Common.Queries;

public interface IQueryWithPassedColumns
{
    SqlToken[] PassedColumns { get; set; }
}