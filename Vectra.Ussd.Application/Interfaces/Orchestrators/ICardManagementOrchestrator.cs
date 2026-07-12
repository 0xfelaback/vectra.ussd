public interface ICardManagementOrchestrator
{
    Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken);
    Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken);
}