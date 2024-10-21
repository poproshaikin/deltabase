using Microsoft.Extensions.Caching.Memory;
using Sql.Tokens;

namespace Db.Core;

internal static class DbCoreExtensions
{
    public static string[] ValuesToString(this SqlToken[] tokens) => tokens.Select(t => t.Value).ToArray();
}