public interface IHandleSession
{
    Task SaveSessionAsync<T>(T session, CancellationToken ct) where T : SessionBase;
    Task RemoveSessionAsync<T>(T session, CancellationToken ct) where T : SessionBase;
}