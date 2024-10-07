using Db.Records;
using Enums.Sql.Queries;
using Enums.Tcp;
using Sql;
using Sql.Core;
using Sql.Queries;
using Utils;

namespace Db.Core;

public class DltDatabase
{
    public string Name { get; private set; }

    private QueryProcessor _qp;
    private QueryParser _parser;
    private FileSystemManager _fs;

    public DltDatabase(string dbName, FileSystemManager fs)
    {
        _fs = fs;
        _qp = new QueryProcessor(_fs, dbName);
        _parser = new QueryParser();
        Name = dbName;
    }

    public byte[] ExecuteRequest(string sql)
    {
        SqlQuery command = _parser.ParseQuery(sql);
        return ExecuteQuery(command);
    }
    
    private byte[] ExecuteQuery(SqlQuery query)
    {
        try
        {
            return query.Type switch
            {
                QueryType.Select => _qp.ExecuteDql((DqlQuery)query),

                _ => throw new NotImplementedException()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return ParseHelper.GetBytes(TcpResponseType.InternalServerError);
        }
    }
}