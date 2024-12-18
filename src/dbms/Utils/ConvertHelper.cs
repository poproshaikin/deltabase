using System.Security.Cryptography;
using System.Text;
using Enums.Exceptions;
using Enums.Network;

namespace Utils;

public static class ConvertHelper
{
    public static byte[] GetBytes(string message) => Encoding.UTF8.GetBytes(message);
    
    public static byte[] GetBytes(ResponseType type) => GetBytes(((int)type).ToString());
    
    public static byte[] GetBytes(ErrorType error) => GetBytes(((int)error).ToString());
    
    public static string GetString(byte[] buffer) => Encoding.UTF8.GetString(buffer);
    
    public static string Sha256(string input)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Encoding.UTF8.GetString(hash);
    }
}