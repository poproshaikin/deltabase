using Enums;
using Enums.Records.Columns;
using Utils;

namespace Db.Records;

/// <summary>
/// Represents the definition of a column in a record, including its name, value type, and constraints.
/// </summary>
public class RecordColumnDef
{
    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string Name;
    
    /// <summary>
    /// Gets the value type of the column.
    /// </summary>
    public ColumnValueType ValueType;
    
    /// <summary>
    /// Gets the constraints associated with the column.
    /// </summary>
    public ColumnConstraint[] Constraints;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordColumnDef"/> class.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="valueType">The value type of the column as a string.</param>
    /// <param name="constraints">An array of constraints for the column.</param>
    public RecordColumnDef(string name, string valueType, string[] constraints)
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
}