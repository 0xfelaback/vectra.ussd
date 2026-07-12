public interface IAirtimeService
{
    Task<AccountQueryDto?> ServiceAccountValidation(string phoneNumber, CancellationToken cancellationToken);
    Task<bool?> VerifyPin1(string providedPin1Hash, string phoneNumber, CancellationToken cancellationToken);
    Task<string?> CreateAirtimeTransaction(string senderPhoneNumber, string beneficiaryPhoneNumber, string remitterAccountNumber,
     decimal amount, AirtimeRecharge.AirtimeNetwork network, AirtimeRecharge.TransactionStatus status,
      bool isSelfRecharge, CancellationToken cancellationToken);
    (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng);
    (EntryResponseDto? errorResponse, string? filteredPhoneNumber) ValidateBeneficiaryPhoneNumber(string phoneNumber);
    Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
}