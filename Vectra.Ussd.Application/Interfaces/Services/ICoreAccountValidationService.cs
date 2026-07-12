public interface ICoreAccountValidationService
{
    Task<(EntryResponseDto? errorResonse, CustomerAccount? userAccount)> AccountValidation(string phoneNumber, string accountNumber, CancellationToken cancellationToken);
}