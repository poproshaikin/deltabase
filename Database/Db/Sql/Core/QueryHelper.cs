using Db.Records;
using Enums.Records.Columns;
using Enums.Sql.Tokens;
using Sql.Common.Queries;
using Sql.Tokens;

namespace Sql.Core;

/// <summary>
/// Provides various helper methods to assist with SQL query processing and value management.
/// </summary>
public class QueryHelper
{
    /// <summary>
    /// Sorts the values based on the order of the columns provided.
    /// </summary>
    /// <param name="columnsInRightOrder_def">The columns in the correct order.</param>
    /// <param name="columnsToSort_t">The columns that need to be sorted.</param>
    /// <param name="valuesToSort_t">The values corresponding to the columns to be sorted.</param>
    /// <returns>A sorted array of values based on the correct column order.</returns>
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
    
    /// <summary>
    /// Validates whether the passed value type matches the expected column value type.
    /// </summary>
    /// <param name="token">The SQL token representing the passed value.</param>
    /// <param name="column">The column definition to compare against.</param>
    /// <returns><c>true</c> if the value type is valid; otherwise, <c>false</c>.</returns>
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
    
    /// <summary>
    /// Retrieves specified column values from the filtered rows.
    /// </summary>
    /// <param name="sourceColumnNames">The source column definitions.</param>
    /// <param name="columnNamesToGetFrom">The columns to extract values from.</param>
    /// <param name="filteredRows">The filtered rows from which values will be retrieved.</param>
    /// <returns>An array of <see cref="RecordRow"/> containing the extracted values.</returns>
    public RecordRow[] GetValuesFromRowsAtColumns(RecordColumnDef[] sourceColumnNames,
        SqlToken[] columnNamesToGetFrom, RecordRow[] filteredRows)
    {
        int[] columnsIndexes = columnNamesToGetFrom.Select(columnName => 
            Array.IndexOf(sourceColumnNames, 
                sourceColumnNames.FirstOrDefault(c => c.Name == columnName))
        ).ToArray();

        return filteredRows.Select(r => new RecordRow(r[columnsIndexes])).ToArray();
    }
    
    /// <summary>
    /// Automatically increments the primary key value for the record if needed.
    /// </summary>
    /// <typeparam name="TQuery">A SQL query type that implements <see cref="IQueryWithPassedColumns"/> and <see cref="IQueryWithPassedValues"/>.</typeparam>
    /// <param name="read">The record to process.</param>
    /// <param name="query">The SQL query that contains the passed columns and values.</param>
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

    /// <summary>
    /// Checks if two arrays are in the same order.
    /// </summary>
    /// <typeparam name="T">The type of elements in the arrays.</typeparam>
    /// <param name="first">The first array to compare.</param>
    /// <param name="second">The second array to compare.</param>
    /// <returns><c>true</c> if the arrays are in the same order; otherwise, <c>false</c>.</returns>
    private bool AreInSameOrder<T>(T[] first, T[] second) where T : class
    {
        if (first.Length != second.Length)
            return false;

        for (int i = 0; i < first.Length; i++)
            if (first[i] != second[i])
                return false;

        return true;
    }
    
    /// <summary>
    /// Determines if the record requires auto-increment for the primary key column.
    /// </summary>
    /// <param name="record">The record to check.</param>
    /// <param name="query">The insert query containing the passed columns.</param>
    /// <returns><c>true</c> if auto-increment is needed; otherwise, <c>false</c>.</returns>
    public bool NeedsAutoIncrement(Record record, InsertQuery query)
    {
        RecordColumnDef pkCol = record.PkColumn!;
        
        SqlToken? passedPkCol = query.PassedColumns.FirstOrDefault(t => t.Value == pkCol.Name);
        return passedPkCol is null && pkCol.HasConstraint(ColumnConstraint.Au);
    }
    
    /// <summary>
    /// Validates whether the passed primary key value matches an existing value in the record.
    /// </summary>
    /// <param name="read">The record to validate against.</param>
    /// <param name="insert">The insert query containing the passed columns and values.</param>
    /// <returns><c>true</c> if the passed primary key value is valid; otherwise, <c>false</c>.</returns>
    public bool IsPassedPkValueValid(Record read, InsertQuery insert)
    {
        SqlToken passedPkCol = insert.PassedColumns.FirstOrDefault(t => t == read.PkColumn!.Name)!;
        int pkColId = read.PkColumnId;
        IEnumerable<string> pkColValues = read.Rows.Select(c => c.Values[pkColId]);
            
        int passedPkValueIndex = Array.IndexOf<SqlToken>(insert.PassedColumns, passedPkCol);
        string passedPkValue = insert.PassedValues[passedPkValueIndex].Value;
        
        return pkColValues.Contains(passedPkValue);
    }
}