using Db.Records;
using Sql.Expressions;
using Sql.Queries;
using Sql.Tokens;
using Utils;

namespace Sql.Core.Evaluating;

public class DqlEvaluator : QueryEvaluator
{
    public DqlEvaluator(FileSystemManager fsManager, string dbName) : base(fsManager, dbName)
    {
    }

    public override QueryResult Evaluate(SqlQuery query)
    {
        DqlQuery dql = (DqlQuery)query;
        
        QueryResult result = EvaluateSelect(dql.Select, dql.From);
        return result;
    }

    private QueryResult EvaluateSelect(SelectExpression select, FromExpression from)
    {
        ArgumentNullException.ThrowIfNull(select);
        ArgumentNullException.ThrowIfNull(from);

        RecordsReader reader = new(FsManager, DbName);
        Record read = reader.Read(from.TableName);
        
        if (select.SelectAllColumns)
        {
            return new QueryResult(read.Rows);
        }
        else
        {
            SqlToken[] columnNames = select.PassedColumns;
            return new QueryResult(read.GetColumns(columnNames.Select(t => t.Value)));
        }
    }

    private QueryResult EvaluateWhere()
    {
        throw new NotImplementedException();
    }
}