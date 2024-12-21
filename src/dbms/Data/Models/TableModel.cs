using System.Data;
using Data.Definitions.Schemes;

namespace Data.Models;

public class TableModel
{
    public TableScheme Scheme { get; set; }
    public PageRow[] Rows { get; set; }

    public TableModel(TableScheme scheme, params PageRow[] rows)
    {
        Scheme = scheme;
        Rows = rows;
    }
}