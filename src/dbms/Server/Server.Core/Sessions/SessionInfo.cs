using Server.Core.Authorization;
using Utils;

namespace Server.Core.Sessions;

internal class SessionInfo
{
    public Guid Id { get; init; }
    public DateTime CreationDate { get; init; }
    public string ServerTokenHash { get; init; }
    public string ClientTokenHash { get; set; }
    public DltDatabase AssociatedDatabase { get; set; }

    public bool IsServerTokenInitialized => ServerTokenHash != null!;
    public bool IsClientTokenInitialized => ClientTokenHash != null!;

    public SessionInfo(SecurityTokenProvider tokenProvider)
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.Now;
        ServerTokenHash = tokenProvider.GetToken(Id, CreationDate);
    }

    public string GetCombinedToken()
    {
        return ConvertHelper.Sha256(ClientTokenHash + ServerTokenHash);
    }

    public bool IsExpired(TimeSpan timeSpan)
    {
        return CreationDate + timeSpan <= DateTime.Now;
    }
}