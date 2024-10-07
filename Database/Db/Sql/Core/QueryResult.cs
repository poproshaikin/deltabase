using Db.Records;
using Utils;

namespace Sql.Core;

public class QueryResult
{
    public RecordRow[] Result { get; private set; }

    public QueryResult(RecordRow[] rows)
    {
        Result = rows;
    }
    
    public byte[] ToBytes()
    {
        return new Record(null!, Result).ToBytes();
    }
}