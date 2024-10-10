using Enums.Records.Columns;
using Enums.Sql.Tokens;
using Db.Records;
using Sql.Queries;
using Sql.Tokens;
using Utils;
using TokenType = Enums.Sql.Tokens.TokenType;

namespace Sql.Core;

public class QueryProcessor
{
    private FileSystemManager _fs;
    private string _dbName;
    private QueryParser _parser;
    
    public QueryProcessor(FileSystemManager fs, string dbName)
    {
        _fs = fs;
        _dbName = dbName;
        _parser = new QueryParser();
    }

    public SqlQuery ParseQuery(string sql) => _parser.Parse(sql);
    
    public string[] SortValues(IEnumerable<RecordColumnDef> columnsInRightOrder_def,
        IEnumerable<SqlToken> columnsToSort_t, IEnumerable<SqlToken> valuesToSort_t)
    {
        string[] columnsInRightOrder = columnsInRightOrder_def.Select(c => c.Name)
            .ToArray();
        string[] columnsToSort = columnsToSort_t.Select(c => c.Value)
            .ToArray();
        string[] valuesToSort = valuesToSort_t.Select(t => t.Value)
            .ToArray();

        if (AreInSameOrder(columnsInRightOrder, columnsToSort))
            return valuesToSort;

        string[] sortedValues = new string[valuesToSort.Length];
        int counter = 0;
        
        for (int i = 0; i < columnsInRightOrder.Length; i++)
        {
            for (int j = 0; j < columnsToSort.Length; j++)
            {
                if (columnsInRightOrder[i] == columnsToSort[j])
                {
                    sortedValues[counter] = valuesToSort[j];
                    counter++;
                }
            }
        }

        return sortedValues;
    }
    
    public bool IsPassedValueTypeValid(SqlToken token, RecordColumnDef column)
    {
        ColumnValueType type = column.ValueType;
        
        if (token.Type == TokenType.NumberLiteral)
        {
            if (type == ColumnValueType.Integer)
                return token.Value.All(c => "123456789".Contains(c));

            if (type == ColumnValueType.Float)
                return token.Value.All(c => "123456789.".Contains(c));
        } 
        else if (token.Type == TokenType.StringLiteral)
        {
            if (type == ColumnValueType.Char)
                return token.Value.Length == 1;

            if (type == ColumnValueType.String)
                return true;

            throw new NotImplementedException();
        }

        throw new NotImplementedException();
    }
    
    public RecordRow[] GetValuesFromRowsAtColumns(RecordColumnDef[] sourceColumnNames,
        SqlToken[] columnNamesToGetFrom, RecordRow[] filteredRows)
    {
        int[] columnsIndexes = columnNamesToGetFrom.Select(columnName => 
            Array.IndexOf(sourceColumnNames, 
                sourceColumnNames.FirstOrDefault(c => c.Name == columnName))
        ).ToArray();

        return filteredRows.Select(r => new RecordRow(r[columnsIndexes])).ToArray();
    }
    
    public void AutoIncrement<TQuery>(Record read, TQuery query) where TQuery : SqlQuery, IQueryWithPassedColumns, IQueryWithPassedValues
    {
        RecordColumnDef pkCol = read.PkColumn!;

        int? value;
        if (read.TryGetLastPkValueInt(out value))
        {
            value++;
        }
        else
        {
            value = 1;
        }
        
        query.PassedValues = query.PassedValues.Prepend(new SqlToken(TokenType.NumberLiteral, value!.ToString()!)).ToArray();
        query.PassedColumns = query.PassedColumns.Prepend(new SqlToken(TokenType.Identifier, pkCol.Name)).ToArray();
    }

    private bool AreInSameOrder<T>(T[] first, T[] second) where T : class
    {
        if (first.Length != second.Length)
            return false;

        for (int i = 0; i < first.Length; i++)
            if (first[i] != second[i])
                return false;

        return true;
    }
    
    public bool NeedsAutoIncrement(Record record, InsertQuery query)
    {
        RecordColumnDef pkCol = record.PkColumn!;
        
        SqlToken? passedPkCol = query.PassedColumns.FirstOrDefault(t => t.Value == pkCol.Name);
        return passedPkCol is null && pkCol.HasConstraint(ColumnConstraint.Ai);
    }
    
    public bool IsPassedPkValueValid(Record read, InsertQuery insert)
    {
        SqlToken passedPkCol = insert.PassedColumns.FirstOrDefault(t => t == read.PkColumn!.Name)!;
        int pkColId = read.PkColumnId;
        IEnumerable<string> pkColValues = read.Rows.Select(c => c.Values[pkColId]);
            
        int passedPkValueIndex = Array.IndexOf<SqlToken>(insert.PassedColumns, passedPkCol);
        string passedPkValue = insert.PassedValues[passedPkValueIndex].Value;
        
        return pkColValues.Contains(passedPkValue);
    }

    // private byte[] ExecuteReader(SqlDqlCommand command, string dbName)
    // {
    //     DltRecordsReader reader = new(_fs, dbName);
    //     DltRecord record = reader.Read(command.GetTableName());
    //
    //     DltRecordRow[] rows;
    //     if (command[1].Type == SqlTokenType.Asterisk)
    //     {
    //         rows = record.Rows.ToArray();
    //     }
    //     else
    //     {
    //         string[] columns = _lexer.Analyze(command, LexingTokenType.Identifier);
    //         int[] rowsX = GetIndexesOfColumns(record, columns);
    //         rows = record.Rows.Select(row => new DltRecordRow(row[rowsX])).ToArray();
    //     }
    //
    //     return SqlParseHelper.RowsToByteArray(rows);
    // }
    //
    // private byte[] ExecuteDmlCommand(SqlDmlCommand dml, string dbName)
    // {
    //     if (dml is SqlInsertCommand insert)
    //     {
    //         return ExecuteInsert(insert, dbName);
    //     }
    //
    //     throw new NotImplementedException();
    // }

    /// <summary>
    /// Parses and then executes the <see cref="SqlInsertCommand"/> in `<see cref="dbName"/>` database
    /// </summary>
    /// <param name="insert"><see cref="SqlInsertCommand"/></param>
    /// <param name="dbName">Name of the database executing command</param>
    /// <returns>Query result in byte array format</returns>
    /// <exception cref="InvalidCastException">"Invalid data was passed while getting the token value"</exception>
    // private byte[] ExecuteInsert(SqlInsertCommand insert, string dbName)
    // {
    //     Logger.Debug("Executing INSERT command");
    //     
    //     string tableName = insert.GetTableName();
    //     DltRecordsReader reader = new(_fs, dbName);
    //     DltRecordsWriter writer = new(_fs, dbName);
    //     DltRecord record = reader.Read(tableName);
    //     DltRecordColumnDef[] columnDefs = record.Columns;
    //
    //     string[] columnsToInsert = GetColumnsToInsert(insert);
    //     string[] valuesToInsert = GetValuesToInsert(insert);
    //
    //     string? pkCol = columnsToInsert.FirstOrDefault(col => columnDefs.Select(colDef => colDef.Name).Contains(col));
    //     bool havePassedDataPkValue = pkCol is not null;
    //
    //     DltRecordRow rowToWrite = new DltRecordRow(columnsToInsert);
    //     
    //     if (!havePassedDataPkValue)
    //     {
    //         string defFilePath = _fs.GetRecordPath(dbName, tableName, FileExtension.DEF);
    //         DltRecordColumnDef? pkColumn = record.GetPkColumn(defFilePath);
    //
    //         if (pkColumn is null) //    Means that the record hasn't a primary key column
    //         {
    //             goto endOfBlock;
    //         }
    //         
    //         string lastPkValue = reader.GetRecordLastPkValue(tableName)!;
    //
    //         if (pkColumn.HasConstraint(ColumnConstraint.Au))
    //         {
    //             int lastPkValueInt = int.Parse(lastPkValue);
    //             lastPkValueInt++;
    //             rowToWrite.Values = rowToWrite.Values.Prepend(lastPkValueInt.ToString()).ToArray();
    //         }
    //     }
    //     // ReSharper disable once BadControlBracesIndent
    // endOfBlock:
    //
    //     string toWritePath = _fs.GetRecordPath(dbName, tableName, FileExtension.RECORD);
    //     writer.Write(tableName, toWritePath, rowToWrite);
    //
    //     return ParseHelper.GetBytes(
    //         ParseHelper.ParseResponseType(TcpResponseType.Success));
    // }
    //
    // private byte[] ExecuteDdlCommand(SqlDdlCommand ddl)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Returns first <see cref="SqlToken"/> that is of the same type as 'type' parameter
    // /// </summary>
    // /// <param name="command"><see cref="SqlCommand"/> where the token will be searched for</param>
    // /// <param name="type"><see cref="SqlTokenType"/> that the searched token should be</param>
    // /// <returns>Index of the first token in command meeting the condition</returns>
    // private SqlToken? GetFirstToken(SqlCommand command, SqlTokenType type)
    // {
    //     return command.FirstOrDefault(t => t.Type == type);
    // }
    //
    // /// <summary>
    // /// Gets an index of the first token in collection that is of the same type as the 'type' parameter
    // /// </summary>
    // /// <param name="command"><see cref="SqlCommand"/> where the token will be searched for</param>
    // /// <param name="type"><see cref="SqlTokenType"/> that the searched token should be</param>
    // /// <param name="orderIndex">Order index in all tokens meeting the condition</param>
    // /// <returns>Index of token in command meeting the condition</returns>
    // private int GetTokenIndex(SqlCommand command, SqlTokenType type, int orderIndex = 0)
    // {
    //     SqlToken? token = GetTokens(command, type).ElementAt(orderIndex);
    //     return command.ToList().IndexOf(token!);
    // }
    //
    // /// <summary>
    // /// Gets all tokens meeting the condition
    // /// </summary>
    // /// <param name="command"><see cref="SqlCommand"/> where the token will be searched for</param>
    // /// <param name="type"><see cref="SqlTokenType"/> that the searched token should be</param>
    // private IEnumerable<SqlToken> GetTokens(SqlCommand command, SqlTokenType type)
    // {
    //     return command.Where(t => t.Type == type);
    // }
    //
    // private int[] GetIndexesOfColumns(DltRecord record, string[] columnNames)
    // {
    //     int[] rowsX = new int[columnNames.Length];
    //
    //     int columnsCounter = 0;
    //     int rowsXCounter = 0;
    //     for (int i = 0; i < record.Columns.Length; i++)
    //     {
    //         if (record.Columns[i].Name == columnNames[columnsCounter])
    //         {
    //             rowsX[rowsXCounter] = i;
    //             
    //             columnsCounter++;
    //             rowsXCounter++;
    //         }
    //     }
    //
    //     return rowsX;
    // }
    //
    // private string[] GetColumnsToInsert(SqlCommand command)
    // {
    //     int leftBracket = GetTokenIndex(command, SqlTokenType.LeftBracket);             
    //     int rightBracket = GetTokenIndex(command, SqlTokenType.RightBracket);
    //
    //     return command[(leftBracket + 1)..(rightBracket - 1)]
    //         .Select(t => t.Value!.ToString() ?? throw new InvalidCastException("Invalid data was passed while getting the column name"))
    //         .ToArray();
    // }
    //
    // private string[] GetValuesToInsert(SqlCommand command)
    // {
    //     int leftBracket = GetTokenIndex(command, SqlTokenType.LeftBracket, 1);             
    //     int rightBracket = GetTokenIndex(command, SqlTokenType.RightBracket, 1);
    //     
    //     return command[(leftBracket + 1)..(rightBracket - 1)]
    //         .Select(t => t.Value!.ToString() ?? throw new InvalidCastException("Invalid data was passed while getting the column name"))
    //         .ToArray();
    // }
}