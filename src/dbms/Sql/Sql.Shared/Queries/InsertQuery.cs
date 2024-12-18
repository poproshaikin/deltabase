using Sql.Shared.Expressions;
using Sql.Shared.Queries.Interfaces;

namespace Sql.Shared.Queries;

public class InsertQuery : ISqlQuery, IParsedQuery, IValidatedQuery
{
    public InsertExpr Insert { get; set; }
    
    public ValuesExpr[] Values { get; set; }

    public InsertQuery()
    {
        Values = [];
    }
}