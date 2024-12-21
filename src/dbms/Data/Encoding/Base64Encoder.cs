    using Utils;

namespace Data.Encoding;

internal class Base64Encoder : IDataEncoder
{
    public string EncodeValue(object value)
    {
        return Convert.ToBase64String(ConvertHelper.GetBytes(value.ToString() ?? string.Empty));
    }

    public string DecodeValue(string value)
    {
        return ConvertHelper.GetString(Convert.FromBase64String(value ?? string.Empty));
    }
}