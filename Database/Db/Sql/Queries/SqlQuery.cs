using System.Collections;
using Enums.Sql.Queries;
using Sql.Expressions;

namespace Sql.Queries;

public abstract class SqlQuery : IEnumerable<SqlExpression>
{
    public SqlExpression[] Expressions { get; private set; }
    public QueryType Type { get; private set; }
    
    public SqlExpression this[int index] => Expressions[index];
    
    protected SqlQuery(IReadOnlyList<SqlExpression> expressions, QueryType type)
    {
        Expressions = expressions.ToArray();
        Type = type;
    }

    public IEnumerator<SqlExpression> GetEnumerator()
    {
        return new ExprEnumerator(Expressions);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

file class ExprEnumerator : IEnumerator<SqlExpression>
{
    private IReadOnlyList<SqlExpression> _expressions;
    private int _current;
    
    public SqlExpression Current => _expressions[_current];
    object? IEnumerator.Current => this.Current;

    public ExprEnumerator(IReadOnlyList<SqlExpression> expressions)
    {
        _expressions = expressions;
    }

    public bool MoveNext()
    {
        _current++;
        return _current < _expressions.Count;       
    }

    public void Reset()
    {
        _current = -1;
    }

    public void Dispose()
    {
        // ignore
    }
}