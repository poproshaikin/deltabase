using Enums.Sql.Tokens;
using Sql.Tokens;

namespace Sql;

internal static class Extensions
{
    public static bool IsNumeric(this string s)
    {
        return double.TryParse(s, out _);
    }
    
    public static int IndexOf(this SqlToken[] tokens, SqlToken? token) => Array.IndexOf(tokens, token);

    public static int IndexOf(this SqlToken[] tokens, Keyword kw) =>
        tokens.IndexOf(tokens.FirstOrDefault(t => t.IsKeyword(kw)));
}