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

    public static TcpCommandType ParseCommandType(string s)
    {
        return s switch
        {
            "~connect" => TcpCommandType.connect,
            "~sql" => TcpCommandType.sql,
            "~test" => TcpCommandType.test,

            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };
    }

    public static ColumnConstraint[] ParseConstraints(params string[] constraints)
    {
        return constraints.Select(ParseConstraint).ToArray();
    }

    public static ColumnConstraint ParseConstraint(string constraint)
    {
        return constraint switch
        {
            "PK" => ColumnConstraint.Pk,
            "NN" => ColumnConstraint.Nn,
            "UN" => ColumnConstraint.Un,
            "AU" => ColumnConstraint.Au,

            _ => throw new ArgumentOutOfRangeException(nameof(constraint), constraint, null)
        };
    }

    public static string ParseConstraint(ColumnConstraint constraint)
    {
        return constraint switch
        {
            ColumnConstraint.Pk => "PK",
            ColumnConstraint.Nn => "NN",
            ColumnConstraint.Un => "UN",
            ColumnConstraint.Au => "AU",

            _ => throw new ArgumentOutOfRangeException(nameof(constraint), constraint, null)
        };
    }

    public static string ParseExtension(FileExtension extension)
    {
        return extension switch
        {
            FileExtension.DEF => "def",
            FileExtension.RECORD => "record",
            FileExtension.CONF => "conf",

            _ => throw new ArgumentOutOfRangeException(nameof(extension), extension, null)
        };
    }

    public static string ParseResponseType(TcpResponseType type)
    {
        return ((int)type).ToString();
    }
}