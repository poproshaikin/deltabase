using Server.Core.Authorization;
using Utils;

namespace Server.Core.Sessions;

internal class AuthService
{
    private readonly TimeSpan _sessionLifetime;
    
    private List<SessionInfo> _sessions;
    private SecurityTokenProvider _tokenProvider;

    public AuthService(TimeSpan sessionLifetime)
    {
        _sessions = [];
        _tokenProvider = new SecurityTokenProvider();
        _sessionLifetime = sessionLifetime;
    }

    public SessionInfo RegisterSession()
    {
        SessionInfo info = new SessionInfo(_tokenProvider);
        _sessions.Add(info);
        return info; 
    }

    public bool TryAuthorizeSession(string authTokenHash, out SessionInfo sessionInfo)
    {
        SessionInfo? associatedSession = GetSession(authTokenHash);

        if (associatedSession is null)
        {
            sessionInfo = null!;
            return false;
        }

        if (associatedSession.IsExpired(_sessionLifetime))
        {
            CloseSession(authTokenHash);
            sessionInfo = null!;
            return false;
        }
        
        sessionInfo = associatedSession;
        return true;
    }

    public bool CloseSession(string authTokenHash)
    {
        SessionInfo? associatedSession = GetSession(authTokenHash);
        
        return associatedSession is not null && _sessions.Remove(associatedSession);
    }
    
    private SessionInfo? GetSession(string authTokenHash) => _sessions.FirstOrDefault(s => s.GetCombinedToken() == authTokenHash);
}