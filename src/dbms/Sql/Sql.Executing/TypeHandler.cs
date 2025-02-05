using Enums.Records.Columns;

namespace Sql.Executing;

internal class TypeHandler
{
    internal object? ConvertToCSharpObject(SqlValueType columnType, string value)
    {
        throw new NotImplementedException();
        return columnType switch
        {
            
        };
    }
    
    internal bool Matches(SqlValueType columnType, string value)
    {
        if (columnType == SqlValueType.String)
            return true;
        
        if (columnType == SqlValueType.Integer)
            return int.TryParse(value, out _);
        
        if (columnType == SqlValueType.Boolean)
            return bool.TryParse(value, out _);
        
        if (columnType == SqlValueType.Float) 
            return float.TryParse(value, out _);
        
        if (columnType == SqlValueType.Char)
            return char.TryParse(value, out _);
        
        // TODO dodelat ostalnyje tipy
        throw new NotImplementedException();
    }
}