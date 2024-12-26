using Enums;
using Enums.Records.Columns;

namespace Data.Definitions.Schemes;

public class ColumnScheme
{
    public string Name { get; set; }
    public SqlValueType ValueType { get; set; }
    public ColumnConstraint[] Constraints { get; set; }
    
    public ColumnScheme(string name, string valueType, string[] constraints)
    {
        Name = name;
        ValueType = EnumsStorage.GetColumnValueType(valueType);
        Constraints = constraints.Select(EnumsStorage.GetConstraint).ToArray();
    }
    
    public bool HasConstraint(ColumnConstraint constraint) => Constraints.Contains(constraint);

    public static ColumnScheme Parse(string recordLine)
    {
        string[] splitted = recordLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        string name = splitted[0];
        string valueType = splitted[1];
        string[] constraints = splitted.Length > 2 ? splitted[2..] : [];

        return new ColumnScheme(name, valueType, constraints);
    }
}