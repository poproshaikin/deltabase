namespace Enums.Records.Columns;

// public enum SqlValueType
// {
//     Null = 1,
//     
//     Integer,
//     String,
//     Char,
//     Float,
//     
//     IntegerArr,
//     StringArr,
//     CharArr,
//     FloatArr,
//     
//     Blob,
// }

public sealed class SqlValueType
{
    // название типа используемое в sql
    public string Name { get; init; }
    
    // возвращает размер типа в байтах, если он не переменный
    public int? Size { get; init; }
    
    // возвращает тип, соответствующий ему в clr, если такой существует
    public Type? ClrType { get; init; }

    public bool IsFixedSize => Size is not null;

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
    public static readonly SqlValueType ByteArr = new("BYTE[]", null, typeof(byte[]));

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
            "BYTE[]" => ByteArr,

            _ => throw new NotImplementedException()
        };
    }
}