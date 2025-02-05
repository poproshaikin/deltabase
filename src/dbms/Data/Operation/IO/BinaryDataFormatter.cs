using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Data.Definitions.Schemes;
using Data.Models;
using Enums.Records.Columns;

namespace Data.Operation.IO;

public class BinaryDataFormatter
{
    public byte[] SerializeRow(PageRow row, TableScheme scheme)
    {
        List<byte> result = [];
        
        result.AddRange(SerializeObject(row.RId, SqlValueType.Integer));
        result.AddRange(row.Data.Select(o => o is null ? (byte)1 : (byte)0));

        for (int i = 0; i < row.ValuesCount; i++)
        {
            SqlValueType valueType = scheme.Columns[i].ValueType;

            if (row[i] is null) continue;

            result.AddRange(SerializeObject(row[i]!, valueType));
        }
        
        return result.Prepend((byte)result.Count).ToArray();
    }
    
    public byte[] SerializeHeader(PageHeader header)
    {
        var result = new List<byte>();
        result.AddRange(SerializeObject(header.PageId, SqlValueType.Integer));
        result.AddRange(SerializeObject(header.RowsCount, SqlValueType.Integer));
        result.AddRange(SerializeObject(header.FreeRows, SqlValueType.IntegerArr));
        return result.ToArray();
    }
    
    public byte[] SerializeObject(object value, SqlValueType valueType)
    {
        return SerializeObject(value.ToString()!, valueType);
    }
    
    public byte[] SerializeObject(string value, SqlValueType valueType)
    {
        if (valueType == SqlValueType.Integer)
            return SerializeAsInt(value);
        
        else if (valueType == SqlValueType.String)
            return SerializeAsString(value);

        else if (valueType == SqlValueType.Boolean)
            return SerializeAsBoolean(value);
        
        else if (valueType == SqlValueType.Float)
            return SerializeAsFloat(value);
        
        else if (valueType == SqlValueType.Byte)
            return SerializeAsByte(value);
        
        else if (valueType == SqlValueType.IntegerArr)
            return SerializeAsArray(value, valueType);

        throw new NotImplementedException();
    }
    
    internal byte[] SerializeAsInt(string value)
    {
        return BitConverter.GetBytes(int.Parse(value));
    }

    internal byte[] SerializeAsString(string value)
    {
        byte[] lengthBytes = BitConverter.GetBytes(value.Length);
        byte[] stringBytes = Encoding.UTF8.GetBytes(value);
        return lengthBytes.Concat(stringBytes).ToArray();
    }

    internal byte[] SerializeAsBoolean(string value)
    {
        return BitConverter.GetBytes(bool.Parse(value));
    }

    internal byte[] SerializeAsFloat(string value)
    {
        return BitConverter.GetBytes(float.Parse(value));
    }

    internal byte[] SerializeAsByte(string value)
    {
        return [byte.Parse(value)];
    }

    internal byte[] SerializeAsArray(string value, SqlValueType valueType)
    {
        throw new NotImplementedException();
    }

    public byte[] SerializeArrayInternal(object array, SqlValueType valueType)
    {
        object[] castedArray = (object[])array;
        
        byte[] bytesOfData = castedArray
            .Aggregate<object, IEnumerable<byte>>([], (sum, current) => sum = sum.Concat(SerializeObject(current, valueType)))
            .ToArray();
        byte[] result = SerializeObject(bytesOfData.Length, SqlValueType.IntegerArr)
            .Concat(bytesOfData)
            .ToArray();
        
        return result;
    }

    public object DeserializeObject(byte[] data, SqlValueType valueType)
    {
        if (valueType == SqlValueType.Integer)
            return DeserializeAsInt(data);
        
        else if (valueType == SqlValueType.String)
            return DeserializeAsString(data);

        else if (valueType == SqlValueType.Boolean)
            return DeserializeAsBoolean(data);
        
        else if (valueType == SqlValueType.Float)
            return DeserializeAsFloat(data);
        
        else if (valueType == SqlValueType.Byte)
            return DeserializeAsByte(data);
        
        else if (valueType == SqlValueType.IntegerArr)
            return DeserializeAsArray(data, valueType);

        throw new NotImplementedException();
        
    }
    
    internal int DeserializeAsInt(byte[] data)
    {
        return BitConverter.ToInt32(data, 0);
    }

    internal string DeserializeAsString(byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }

    internal bool DeserializeAsBoolean(byte[] data)
    {
        return BitConverter.ToBoolean(data, 0);
    }

    internal float DeserializeAsFloat(byte[] data)
    {
        return BitConverter.ToSingle(data, 0);
    }

    internal byte DeserializeAsByte(byte[] data)
    {
        return data[0];
    }
    
    public char DeserializeAsChar(byte[] data)
    {
        return BitConverter.ToChar(data, 0);
    }

    internal object[] DeserializeAsArray(byte[] data, SqlValueType valueType)
    {
        if (valueType.IsFixedSize)
        {
            int elementSize = valueType.Size!.Value!;
            int elementsCount = data.Length / elementSize;

            object[] result = new object[elementsCount];
            for (int i = 0; i < elementsCount; i++)
            {
                byte[] objectBytes = data.Skip(i * elementSize).Take(elementSize).ToArray();
                result[i] = DeserializeObject(objectBytes, valueType);
            }

            return result;
        }
        else 
        {
            int offset = 0;
            List<object> result = [];

            int i = 0;
            while (i < data.Length - 1)
            {
                int length = DeserializeAsInt(data[i..sizeof(int)]);
                offset += sizeof(int);
                
                byte[] elementBytes = data.Skip(offset).Take(length).ToArray();
                result.Add(DeserializeObject(elementBytes, valueType));

                i++;
            }

            return result.ToArray();
        }
    }
    
    public ulong EstimateSize(TableScheme scheme, string[] values)
    {
        ulong totalSize = 0;

        totalSize += sizeof(int); // row id
        totalSize += (ulong)values.Length * sizeof(byte); // null bitmap
        
        SqlValueType[] valueTypes = scheme.Columns.Select(v => v.ValueType).ToArray();
        for (int i = 0; i < valueTypes.Length; i++)
        {
            totalSize += EstimateSize(values[i], valueTypes[i]);
        }

        return totalSize;
    }

    public ulong EstimateSize(string objValue, SqlValueType valueType)
    {
        if (valueType.IsFixedSize)
        {
            return (ulong)valueType.Size!;
        }

        return EstimateSizeNonFixed(objValue, valueType);
    }

    public ulong EstimateSizeNonFixed(string objValue, SqlValueType valueType)
    {
        if (valueType == SqlValueType.String)
        {
            return (ulong)(sizeof(int) + objValue.Length);
        }

        throw new NotImplementedException();
    }

    //
    // public byte[] SerializeInt(object value)
    // {
    //     if (value is int @int)
    //         return BitConverter.GetBytes(@int);
    //
    //     if (value is string @string && int.TryParse(@string, out int result))
    //         return SerializeInt(result);
    //
    //     throw new NotImplementedException();
    // }
    //
    // public int DeserializeInt(byte[] bytes)
    // {
    //     return BitConverter.ToInt32(bytes, 0);
    // }
    //
    //
    // public byte[] SerializeFloat(object value)
    // {
    //     if (value is float @float)
    //         return BitConverter.GetBytes(@float);
    //     
    //     if (value is string @string && float.TryParse(@string, out float result))
    //         return SerializeFloat(result);
    //
    //     throw new NotImplementedException();
    // }
    //
    // public float DeserializeFloat(byte[] bytes)
    // {
    //     return BitConverter.ToSingle(bytes, 0);
    // }
    //
    //
    // public byte[] SerializeString(object value)
    // {
    //     byte[] stringBytes = Encoding.UTF8.GetBytes(value);
    //     byte[] lengthBytes = SerializeInt(stringBytes.Length);
    //     return lengthBytes.Concat(stringBytes).ToArray();
    // }
    //
    // public string DeserializeString(byte[] bytes)
    // {
    //     return Encoding.UTF8.GetString(bytes);
    // }
    //
    //
    // public byte[] SerializeChar(char value)
    // {
    //     return BitConverter.GetBytes(value);
    // }
    //
    // public char DeserializeChar(byte[] bytes)
    // {
    //     return BitConverter.ToChar(bytes);
    // }
    //
    //
    // public byte[] SerializeBoolean(bool value)
    // {
    //     return BitConverter.GetBytes(value);
    // }
    //
    // public bool DeserializeBoolean(byte[] bytes)
    // {
    //     return BitConverter.ToBoolean(bytes, 0);
    // }
    //
    //
    // public byte[] SerializeArray(object value)
    // {
    //     object[] array = (object[])value;
    //     
    //     byte[] bytesOfData = array
    //         .Aggregate<object, IEnumerable<byte>>([], (sum, current) => sum = sum.Concat(SerializeObject(current)))
    //         .ToArray();
    //     byte[] result = SerializeInt(bytesOfData.Length)
    //         .Concat(bytesOfData)
    //         .ToArray();
    //     
    //     return result;
    // }
    //
    //
    // public byte[] AddLength(byte[] origin)
    // {
    //     return SerializeInt(origin.Length).Concat(origin).ToArray();
    // }
    //
    // public byte[] RemoveLength(byte[] origin)
    // {
    //     return origin[sizeof(int)..];
    // }
    //
    //
    // public bool TrySerializeObject(object value, out byte[] result)
    // {
    //     try
    //     {
    //         result = SerializeObject(value);
    //         return true;
    //     }
    //     catch (NotImplementedException)
    //     {
    //         result = null!;
    //         return false;
    //     }
    // }
    //
    // public byte[] SerializeObject(object value)
    // {
    //     return value switch
    //     {
    //         int @int => SerializeInt(@int),
    //         float @float => SerializeFloat(@float),
    //         char @char => SerializeChar(@char),
    //         bool @bool => SerializeBoolean(@bool),
    //         string @string => SerializeString(@string),
    //         Array array => SerializeArray(array),
    //         
    //         // у массивов в начале нужно добавлять длину масива в ?элементах/байтах
    //         
    //         // int[] intArray => intArray.Aggregate<int, byte[]>([],
    //         //     (sum, current) => sum.Concat(BitConverter.GetBytes(current)).ToArray()),
    //         //
    //         // float[] floatArray => floatArray.Aggregate<float, byte[]>([],
    //         //     (sum, current) => sum.Concat(BitConverter.GetBytes(current)).ToArray()),
    //         //
    //         // string[] stringArray => stringArray.Aggregate<string, byte[]>([],
    //         //     (sum, current) => sum.Concat(Encoding.UTF8.GetBytes(current)).ToArray()),
    //         
    //         _ => throw new NotImplementedException()
    //     };
    // }
    //
    //
    // public T DeserializeObject<T>(byte[] bytes)
    // {
    //     if (typeof(T) == typeof(int))
    //         return (T)(object)DeserializeInt(bytes);
    //     
    //     if (typeof(T) == typeof(float))
    //         return (T)(object)DeserializeFloat(bytes);
    //     
    //     if (typeof(T) == typeof(string))
    //         return (T)(object)DeserializeString(bytes);
    //     
    //     if (typeof(T) == typeof(bool))
    //         return (T)(object)DeserializeBoolean(bytes);
    //     
    //     if (typeof(T) == typeof(char))
    //         return (T)(object)DeserializeChar(bytes);
    //
    //     throw new NotImplementedException();
    // }
    //
    // public unsafe T[] DeserializeArray<T>(byte[] bytes)
    // {
    //     if (typeof(T).IsUnmanaged())
    //     {
    //         int elementSize = Unsafe.SizeOf<T>();
    //         int elementsCount = bytes.Length / elementSize;
    //
    //         T[] result = new T[elementsCount];
    //         for (int i = 0; i < elementsCount; i++)
    //         {
    //             byte[] objectBytes = bytes.Skip(i * elementSize).Take(elementSize).ToArray();
    //             result[i] = DeserializeObject<T>(objectBytes);
    //         }
    //     }
    //     else
    //     {
    //         
    //     }
    // }
    //
}

file static class Extensions
{
    public static bool IsUnmanaged(this Type type)
    {
        // primitive, pointer or enum -> true
        if (type.IsPrimitive || type.IsPointer || type.IsEnum)
            return true;

        // not a struct -> false
        if (!type.IsValueType)
            return false;

        // otherwise check recursively
        return type
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .All(f => IsUnmanaged(f.FieldType));
    }
}