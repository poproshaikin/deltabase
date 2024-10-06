using Enums;
using Enums.Records.Columns;
using Utils;

namespace Db.Records;

public class RecordColumnDef
{
    public string Name;
    public ColumnValueType ValueType;
    public ColumnConstraint[] Constraints;

    public RecordColumnDef(string name, string valueType, string[] constraints)
    {
        Name = name;
        ValueType = EnumsStorage.GetColumnValueType(valueType);
        Constraints = ParseHelper.ParseConstraints(constraints);
    }

    public bool HasConstraint(ColumnConstraint constraint) => Constraints.Contains(constraint);
}