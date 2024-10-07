using Sql.Queries;
using Utils;

namespace Sql.Core.Evaluating;

public abstract class QueryEvaluator
{
    protected FileSystemManager FsManager;
    protected string DbName;

    protected QueryEvaluator(FileSystemManager fsManager, string dbName)
    {
        FsManager = fsManager;
        DbName = dbName;
    }
    
    public abstract QueryResult Evaluate(SqlQuery query);
}