public interface IDataRechargeService
{
    Task<IEnumerable<DataBundleQueryDto>?> GetActiveBundlesforTelco(AirtimeRecharge.AirtimeNetwork network, CancellationToken token);
    Task<bool?> VerifyPin1(string providedPin1Hash, string phoneNumber, CancellationToken cancellationToken);
    (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng);
    Task<string?> DataPurchase(string senderPhoneNumber, string beneficiaryPhoneNumber, string remitterAccountNumber,
     decimal amount, AirtimeRecharge.AirtimeNetwork network, AirtimeRecharge.TransactionStatus status,
      bool isSelfRecharge, CancellationToken cancellationToken);
    Task<AccountQueryDto?> ServiceAccountValidation(string phoneNumber, CancellationToken cancellationToken);
    Task<bool> DebitAccountAsync(string accountNumber, decimal amount, CancellationToken cancellationToken);
    Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
}