namespace Data.Encoding;

public interface IDataEncoder
{
    public string EncodeValue(object value);
    
    public string DecodeValue(string value);

    static IDataEncoder? TryGet(string? type)
    {
        return type switch
        {
            "base64" => new Base64Encoder(),

            _ => null
        };
    }
}