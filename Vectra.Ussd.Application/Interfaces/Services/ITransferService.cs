using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Application.Interfaces.Services;

public interface ITransferService
{
    Task<string?> GetAccountNumbersByPhoneAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<IEnumerable<ServiceAccount>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
    Task<AccountQueryDto?> AccountNumberValidation(string accountNumber, CancellationToken cancellationToken);
    Task<AccountQueryDto?> ServiceAccountValidation(string phoneNumber, CancellationToken cancellationToken);
    Task<BvnQueryDto?> BvnValidation(string bvnNumber, CancellationToken token);
    Task<NubanQueryDto?> GetIntrabankTransferBeneficiaryDetails(string accountNumber, CancellationToken token);
    Task<string?> GetBankNameByCode(string bankCode, CancellationToken cancellationToken);
    Task<bool> InActivateServiceAccount(string accountNumber, CancellationToken token);
    bool BalanceCheck(decimal transferAmount, decimal remitterAccountBalance);
    Task<string?> CheckAccountTierLimitExceeded(string accountNumber, decimal transferAmount, int tierLevel,
     TransferType transferType, CancellationToken cancellationToken);
    Task IncrementAccountPinTrial(string accountNumber, CancellationToken cancellationToken);
    Task ResetAccountPinTrial(string accountNumber, CancellationToken cancellationToken);
    Task<string> ExecuteTransfer(string remitterAccountNumber, string beneficiaryAccountNumber, decimal transferAmount,
     TransferType transferType, TransactionHistory.TransactionStatus transactionStatus, string? failureReason, string bankName,
      string idempotencyKey, CancellationToken cancellationToken);
    Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
}