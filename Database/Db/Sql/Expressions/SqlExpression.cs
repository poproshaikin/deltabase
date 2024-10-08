using Enums;
using Enums.Sql.Tokens;
using Sql.Tokens;

namespace Sql.Expressions;

public abstract class SqlExpression
{
    public SqlToken[] Tokens { get; protected set; }

    protected SqlExpression(IEnumerable<SqlToken> tokens) : this(tokens.ToArray())
    {
    }

    private SqlExpression(SqlToken[] tokens)
    {
        Tokens = tokens;
    }

    public int IndexOf(SqlToken token) => Array.IndexOf(Tokens, token);

    public int IndexOf(Keyword kw)
    {
        string keywordStr = EnumsStorage.GetKeywordString(kw);
        SqlToken found = Tokens.FirstOrDefault(t => t == keywordStr)!;
        return IndexOf(found);
    }
}