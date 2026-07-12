public interface IRegistrationService
{
    Task<AccountQueryDto?> ServiceAccountValidation(string phoneNumber, CancellationToken cancellationToken);
    Task<AccountQueryDto?> AccountNumberValidation(string accountNumber, CancellationToken cancellationToken);
    Task<BvnQueryDto?> BvnValidation(string bvnNumber, CancellationToken token);
    Task<int> CreateUSSDPin1(string accountNumber, string PinHash, CancellationToken cancellationToken);
    Task<CardQueryDto?> GetOneCardFromAccount(string accountNumber, CancellationToken cancellationToken);
    Task RegisterUssd(string accountNumber, CancellationToken cancellationToken);
    Task<bool> ValidateLastSixDigits(string sixDigits, string accountNumber, CancellationToken cancellationToken);
    (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng);
    Task<EntryResponseDto?> VerifyPinConfirmation<T>(T session, string ussdPinHash, string providedPassword, CancellationToken cancellationToken) where T : SessionBase;
    Task LinkInitialAccount(string phoneNumber, string accountNumber, string pinHash, CancellationToken cancellationToken);
}