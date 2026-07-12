using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

public class VerifySession : IVerifySession
{
    private readonly IDistributedCache _cache;
    public VerifySession(IDistributedCache cache)
    {
        _cache = cache;
    }
    public async Task<(EntryResponseDto? error, T? session)> VerifySessionAsync<T>(SessionBase sessionBase, CancellationToken cancellationToken) where T : SessionBase
    {
        string sessionJson = await _cache.GetStringAsync(sessionBase.SessionId, cancellationToken) ?? string.Empty;
        if (string.IsNullOrEmpty(sessionJson)) return (new EntryResponseDto("This session does not exist.", UssdMessageType.EndSession), null);
        T? session = JsonSerializer.Deserialize<T>(sessionJson) ?? null;
        if (session is null) return (new EntryResponseDto("This session does not exist.", UssdMessageType.EndSession), null);
        return (null, session);
    }
}

