using Microsoft.AspNetCore.Identity;

public interface IServiceAccountNumbersService
{
    Task<PasswordVerificationResult> ValidatePinAsync(string phoneNumber, string enteredPin, CancellationToken cancellationToken);
    Task<string?> GetAccountNumbersByPhoneAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<bool> DebitAccountAsync(string accountNumber, decimal amount, CancellationToken cancellationToken);
    Task<EntryResponseDto?> VerifyUserPINWithTrials<T>(T session, int maxPinTrials, string phoneNumber, string enteredPin,
     CancellationToken cancellationToken) where T : ServiceAccountNumbersSession;
    Task<(EntryResponseDto? errorMsg, string? userAccountNumber)> DebitCustomer<T>(T session, string phoneNumber, decimal queryAmount, CancellationToken cancellationToken) where T : ServiceAccountNumbersSession;
    Task<IEnumerable<ServiceAccount>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
    Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken);

}
