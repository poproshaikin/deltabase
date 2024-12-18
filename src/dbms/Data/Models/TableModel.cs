using System.Data;
using Data.Definitions.Schemes;

namespace Data.Models;

public class TableModel
{
    public TableScheme Scheme { get; set; }
    public RowModel[] Rows { get; set; }

    public TableModel(TableScheme scheme, params RowModel[] rows)
    {
        Scheme = scheme;
        Rows = rows;
    }
}