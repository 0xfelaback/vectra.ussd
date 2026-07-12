using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

public class HandleSession : IHandleSession
{
    private readonly IDistributedCache _cache;
    public HandleSession(IDistributedCache cache)
    {
        _cache = cache;
    }
    public Task SaveSessionAsync<T>(T session, CancellationToken ct) where T : SessionBase
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        return _cache.SetStringAsync(session.SessionId, JsonSerializer.Serialize(session), options, ct);
    }
    public Task RemoveSessionAsync<T>(T session, CancellationToken ct) where T : SessionBase
    {
        return _cache.RemoveAsync(session.SessionId, ct);
    }
}



