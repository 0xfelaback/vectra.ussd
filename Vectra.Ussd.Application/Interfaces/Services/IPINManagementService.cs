public interface IPINManagementService
{
    Task<IEnumerable<AccountQueryDto>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
    Task<bool> ValidateLastSixDigits(string sixDigits, string accountNumber, CancellationToken cancellationToken);
    Task<BvnQueryDto?> BvnValidation(string bvnNumber, CancellationToken token);
    Task<BvnQueryDto?> BvnValidationByPhoneNumber(string phoneNumber, CancellationToken token);
    Task<CardQueryDto?> GetOneCardFromAccount(string accountNumber, CancellationToken cancellationToken);
    Task CreateUSSDPin1(string accountNumber, string PinHash, CancellationToken cancellationToken);
    Task CreateUSSDPin2(string accountNumber, string PinHash, CancellationToken cancellationToken);
    Task EditUSSDPin1(string accountNumber, string PinHash, CancellationToken cancellationToken);
    Task EditUSSDPin2(string accountNumber, string PinHash, CancellationToken cancellationToken);
    Task<bool> VerifyPin1(string providedPin1Hash, string phoneNumber, CancellationToken cancellationToken);
    Task<bool> VerifyPin2(string providedPin2Hash, string phoneNumber, CancellationToken cancellationToken);
    (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng);
    EntryResponseDto? VerifyPinConfirmation(string ussdPinHash, string providedPassword);
}