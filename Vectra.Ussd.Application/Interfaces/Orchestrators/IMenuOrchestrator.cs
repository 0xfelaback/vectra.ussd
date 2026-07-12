public interface IMenuOrchestrator
{
    Task<EntryResponseDto> InitialPageRequest(EntryRequestDto requestDto, CancellationToken cancellationToken);
    Task<EntryResponseDto> NextPageRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken);
}