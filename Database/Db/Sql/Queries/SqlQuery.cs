using Enums.Sql.Tokens;
using Sql.Tokens;

namespace Sql.Queries;

public abstract class SqlQuery
{
    public SqlToken[] Tokens { get; init; }

    protected SqlQuery(IReadOnlyList<SqlToken> tokens)
    {
        Tokens = tokens.ToArray();
    }

    public int IndexOf(Keyword kw)
    {
        return Array.IndexOf<SqlToken>(Tokens, Tokens.FirstOrDefault(t => t.IsKeyword(kw))!);
    }
}