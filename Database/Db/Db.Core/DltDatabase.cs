using Db.Records;
using Enums.Sql.Queries;
using Enums.Tcp;
using Sql;
using Sql.Common.Queries;
using Sql.Core;
using Utils;

namespace Db.Core;

/// <summary>
/// Represents a database that processes SQL queries.
/// </summary>
public class DltDatabase
{
    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public string Name { get; private set; }

    private QueryProcessor _qp;
    private QueryParser _parser;
    private FileSystemManager _fs;

    /// <summary>
    /// Initializes a new instance of the <see cref="DltDatabase"/> class with the specified database name and file system manager.
    /// </summary>
    /// <param name="dbName">The name of the database.</param>
    /// <param name="fs">The file system manager used for managing database files.</param>
    public DltDatabase(string dbName, FileSystemManager fs)
    {
        _fs = fs;
        _qp = new QueryProcessor(_fs, dbName);
        _parser = new QueryParser();
        Name = dbName;
    }

    /// <summary>
    /// Executes the given SQL query and returns the result as a byte array.
    /// </summary>
    /// <param name="sql">The SQL query to be executed.</param>
    /// <returns>A byte array containing the result of the query.</returns>
    public byte[] ExecuteRequest(string sql)
    {
        SqlQuery query = _parser.Parse(sql);
        return _qp.ParseQuery()
    }
    
    
}