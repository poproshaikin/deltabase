using Data.Definitions.Schemes;
using Data.Operation;
using Sql.Shared.Expressions;

namespace Data.Models;

internal struct PageReadingPlan
{
    internal PageInfo PageToRead { get; set; }
    
    internal TableScheme Scheme { get; set; }

    internal int? RowsToReadCount { get; set; }    // if null, read all rows
    
    internal string[]? PassedColumns { get; set; }
    
    internal ConditionGroup? ConditionGroup { get; set; }

    internal bool IsHeaderRead { get; set; }

    internal PageReadingPlan(PageInfo pageToRead,
        TableScheme scheme,
        int? rowsToReadCount,
        string[]? passedColumns,
        ConditionGroup? conditionGroup,
        bool isHeaderRead)
    {
        PageToRead = pageToRead;
        Scheme = scheme;
        RowsToReadCount = rowsToReadCount;
        PassedColumns = passedColumns;
        ConditionGroup = conditionGroup;
        IsHeaderRead = isHeaderRead;
    }
}