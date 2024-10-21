using Enums;
using Enums.Records.Columns;

namespace Db.DataStorage;

public class ColumnSchema
{
    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Gets the value type of the column.
    /// </summary>
    public ColumnValueType ValueType { get; private set; }
    
    /// <summary>
    /// Gets the constraints associated with the column.
    /// </summary>
    public ColumnConstraint[] Constraints { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordColumnDef"/> class.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="valueType">The value type of the column as a string.</param>
    /// <param name="constraints">An array of constraints for the column.</param>
    public ColumnSchema(string name, string valueType, string[] constraints)
    {
        Name = name;
        ValueType = EnumsStorage.GetColumnValueType(valueType);
        Constraints = constraints.Select(EnumsStorage.GetConstraint).ToArray();
    }

    /// <summary>
    /// Checks if the column has a specified constraint.
    /// </summary>
    /// <param name="constraint">The constraint to check.</param>
    /// <returns><c>true</c> if the column has the specified constraint; otherwise, <c>false</c>.</returns>
    public bool HasConstraint(ColumnConstraint constraint) => Constraints.Contains(constraint);

    public static ColumnSchema Parse(string recordLine)
    {
        string[] split = recordLine.Split(' ');
        
        string name = split[0];
        string valueType = split[1];
        string[] constraints = split.Length > 2 ? split[2..] : [];

        return new ColumnSchema(name, valueType, constraints);
    }
}