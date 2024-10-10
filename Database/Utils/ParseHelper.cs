using System.Text;
using Enums.FileSystem;
using Enums.Records.Columns;
using Enums.Tcp;

namespace Utils;

public static class ParseHelper
{
    public static byte[] GetBytes(string message) => Encoding.UTF8.GetBytes(message);
    public static byte[] GetBytes(TcpResponseType type) => GetBytes(((int)type).ToString());
    public static string GetString(byte[] buffer) => Encoding.UTF8.GetString(buffer);
}