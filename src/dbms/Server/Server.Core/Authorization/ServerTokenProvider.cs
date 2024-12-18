using System.Globalization;
using System.Security.Cryptography;
using Server.Core.Sessions;
using Utils;

namespace Server.Core.Authorization;

internal class SecurityTokenProvider
{
    public string GetToken(Guid id, DateTime creationDate)
    {
        string info = id.ToString() + creationDate.ToString(CultureInfo.CurrentCulture);
        return ConvertHelper.Sha256(info);
    }
}