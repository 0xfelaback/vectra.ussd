public interface IVerifySession
{
    Task<(EntryResponseDto? error, T? session)> VerifySessionAsync<T>(SessionBase sessionBase, CancellationToken cancellationToken) where T : SessionBase;
}