using Sql.Tokens;

namespace Sql.Queries;

public abstract class SqlQuery
{
    public SqlToken[] Tokens { get; init; }

    protected SqlQuery(IReadOnlyList<SqlToken> tokens)
    {
        Tokens = tokens.ToArray();
    }
}