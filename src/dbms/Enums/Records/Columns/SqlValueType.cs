namespace Enums.Records.Columns;

public readonly struct SqlValueType : IEquatable<SqlValueType>
{
    /// <summary>
    /// Name of the type used in SQL
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    /// Byte-size of the type, if isn't variable
    /// </summary>
    public int? Size { get; init; }
    
    /// <summary>
    /// Corresponding type in CLR, if exists
    /// </summary>
    public Type? ClrType { get; init; }

    public bool IsFixedSize => Size.HasValue;

    private SqlValueType(string name, int? size, Type? clrType)
    {
        Name = name;
        Size = size;
        ClrType = clrType;
    }

    public static readonly SqlValueType Null = new("NULL", null, null);
    
    public static readonly SqlValueType Integer = new("INTEGER", sizeof(int), typeof(int));
    public static readonly SqlValueType IntegerArr = new("INTEGER[]", null, typeof(int[]));
    
    public static readonly SqlValueType Boolean = new("BOOLEAN", sizeof(bool), typeof(bool));
    public static readonly SqlValueType BooleanArr = new("BOOLEAN[]", null, typeof(bool[]));
    
    public static readonly SqlValueType String = new("STRING", null, typeof(string));
    public static readonly SqlValueType StringArr = new("STRING[]", null, typeof(string[]));
    
    public static readonly SqlValueType Char = new("CHAR", sizeof(char), typeof(char));
    public static readonly SqlValueType CharArr = new("CHAR[]", null, typeof(char[]));
    
    public static readonly SqlValueType Float = new ("FLOAT", sizeof(float), typeof(float));
    public static readonly SqlValueType FloatArr = new("FLOAT[]", null, typeof(float[]));
    
    public static readonly SqlValueType Byte = new("BYTE", sizeof(byte), typeof(byte));
    public static readonly SqlValueType Binary = new("BINARY", null, typeof(byte[]));

    public static SqlValueType Parse(string valueType)
    {
        return valueType.ToUpper() switch
        {
            "NULL" => Null,

            "INTEGER" => Integer,
            "INTEGER[]" => IntegerArr,

            "BOOLEAN" => Boolean,
            "BOOLEAN[]" => BooleanArr,

            "STRING" => String,
            "STRING[]" => StringArr,

            "CHAR" => Char,
            "CHAR[]" => CharArr,

            "FLOAT" => Float,
            "FLOAT[]" => FloatArr,

            "BYTE" => Byte,
            "BINARY" => Binary,

            _ => throw new NotImplementedException()
        };
    }

    public static bool operator ==(SqlValueType left, SqlValueType right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SqlValueType left, SqlValueType right)
    {
        return !Equals(left, right);
    }
    
    public bool Equals(SqlValueType other)
    {
        // it's safe because all the types are static-defined and have private constructor
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return obj is SqlValueType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}