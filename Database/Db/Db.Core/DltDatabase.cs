using Db.Records;
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
        Name = dbName;
    }

    public byte[] ExecuteRequest(string sql)
    {
        SqlQuery command = _parser.ParseQuery(sql);
        return ExecuteQuery(command);
    }
    
    private byte[] ExecuteQuery(SqlQuery command)
    {
        try
        {
            return command switch
            {
                SelectQuery select => ExecuteReader(select),
                InsertQuery insert => ExecuteInsert(insert),
                UpdateQuery update => ExecuteUpdate(update),
                DeleteQuery delete => ExecuteDelete(delete),

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