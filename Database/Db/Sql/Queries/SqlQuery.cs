using Sql.Tokens;

namespace Sql.Common.Queries;

public abstract class SqlQuery
{
    public SqlToken[] Tokens { get; init; }

    protected SqlQuery(IReadOnlyList<SqlToken> tokens)
    {
        Tokens = tokens.ToArray();
    }
}