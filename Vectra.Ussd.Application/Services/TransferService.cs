using AutoMapper;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;
using Vectra.Ussd.Domain.Entities.CoreBanking;
using Vectra.Ussd.Application.Interfaces.Services;


public sealed class TransferService : ITransferService
{
    private readonly ICustomerAccountRepository _customerAccountRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IBvnReadRepository _repository;
    private readonly INubanRepository _nubanRepository;
    private readonly IAccountTierRepository _accountTierRepository;
    private readonly ITransactionHistoryRepository _transactionHistoryRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    public TransferService(IMapper mapper, IServiceAccountRepository serviceAccountRepository, ITransactionHistoryRepository transactionHistoryRepository, ICustomerRepository customerRepository, IBvnReadRepository repository, INubanRepository nubanRepository, ICustomerAccountRepository customerAccountRepository, IAccountTierRepository accountTierRepository)
    {
        _customerAccountRepository = customerAccountRepository; _serviceAccountRepository = serviceAccountRepository;
        _repository = repository;
        _nubanRepository = nubanRepository;
        _accountTierRepository = accountTierRepository;
        _transactionHistoryRepository = transactionHistoryRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
    }
    public async Task<string?> GetAccountNumbersByPhoneAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (account is null) return null;
        return account.AccountNumber;
    }
    public async Task<IEnumerable<ServiceAccount>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceAccount?> accounts = await _serviceAccountRepository.GetAccountsByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (!accounts.Any()) return null;
        return accounts!;
    }
    public async Task<AccountQueryDto?> AccountNumberValidation(string accountNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);
        if (account is null) return null;
        return _mapper.Map<AccountQueryDto>(account);
    }
    public async Task<AccountQueryDto?> ServiceAccountValidation(string phoneNumber, CancellationToken cancellationToken)
    {
        /* WHAT IS THIS METHOD FOR EXACTLY ? */
        ServiceAccount? account = await _serviceAccountRepository.GetFirstAccountByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (account is null) return null;
        return _mapper.Map<AccountQueryDto>(account);
    }
    public async Task<BvnQueryDto?> BvnValidation(string bvnNumber, CancellationToken token)
    {
        MockBvnRecord? bvnRecord = await _repository.QueryBvnByBVN(bvnNumber, token);
        if (bvnRecord is null) return null;
        return _mapper.Map<BvnQueryDto>(bvnRecord);
    }
    public async Task<NubanQueryDto?> GetIntrabankTransferBeneficiaryDetails(string accountNumber, CancellationToken token)
    {
        var nubanResult = await _nubanRepository.GetByAccountNumberAsync(accountNumber, token);
        if (nubanResult is null) return null;
        return _mapper.Map<NubanQueryDto>(nubanResult);
    }

    public async Task<string?> GetBankNameByCode(string bankCode, CancellationToken cancellationToken)
    {
        var banks = await _nubanRepository.GetByBankCodeAsync(bankCode, cancellationToken);
        foreach (var bank in banks)
        {
            return bank.BankName;
        }
        return null;
    }
    public async Task<bool> InActivateServiceAccount(string accountNumber, CancellationToken token)
    {
        int result = await _serviceAccountRepository.MakeAccountDormant(accountNumber, token);
        await _customerAccountRepository.SaveChangesAsync(token);
        if (result < 1) return false;
        return true;
    }
    public bool BalanceCheck(decimal transferAmount, decimal remitterAccountBalance)
    {
        decimal charge;
        if (transferAmount < 6000)
        {
            charge = 10m;
        }
        else if (transferAmount >= 6000 && transferAmount < 50000)
        {
            charge = 25m;
        }
        else
        {
            charge = 50m;
        }
        decimal totalDebit = transferAmount + charge;
        return remitterAccountBalance >= totalDebit;
    }
    public async Task<string?> CheckAccountTierLimitExceeded(string accountNumber, decimal transferAmount, int tierLevel, TransferType transferType, CancellationToken cancellationToken)
    {
        IEnumerable<TransactionHistory> history = await _transactionHistoryRepository.GetAccountTransactionsAsync(accountNumber, DateTime.Now, DateTime.Now.AddDays(-1), cancellationToken);
        AccountTier? remitterTier = await _accountTierRepository.GetTierLimitAsync(tierLevel, transferType, cancellationToken);
        if (remitterTier is null) return null;
        if (!remitterTier.IsActive) return null;
        var transactedAmount = history.Sum(t => t.Amount);

        if (transferAmount > remitterTier.SingleTransactionLimit)
        {
            return $"This amount exceeds your single transaction limit of {remitterTier.SingleTransactionLimit}.";
        }
        else if (history.Count() > remitterTier.DailyTransactionCount)
        {
            return $"You've reached your limit of {remitterTier.DailyTransactionCount} transfers for today.";
        }
        else if (transactedAmount > remitterTier.DailyTransactionLimit)
        {
            return "You have reached your daily transfer limit.";
        }
        else if (transactedAmount + transferAmount > remitterTier.DailyTransactionLimit)
        {
            return $"This transfer would put you over your daily spending limit of {remitterTier.DailyTransactionLimit}. Please try a smaller amount or wait until tomorrow.";
        }
        return null;
    }
    public async Task IncrementAccountPinTrial(string accountNumber, CancellationToken cancellationToken)
    {
        await _customerRepository.IncrementPinTrial(accountNumber, cancellationToken);
        await _customerAccountRepository.SaveChangesAsync(cancellationToken);
    }
    public async Task ResetAccountPinTrial(string accountNumber, CancellationToken cancellationToken)
    {
        await _customerRepository.ResetPinTrial(accountNumber, cancellationToken);
        await _customerAccountRepository.SaveChangesAsync(cancellationToken);
    }
    public async Task<string> ExecuteTransfer(string remitterAccountNumber, string beneficiaryAccountNumber, decimal transferAmount,
     TransferType transferType, TransactionHistory.TransactionStatus transactionStatus, string? failureReason, string bankName, string idempotencyKey,
      CancellationToken cancellationToken)
    {
        return _transactionHistoryRepository.AddTransaction(remitterAccountNumber, beneficiaryAccountNumber, transferAmount, transferType, transactionStatus, failureReason, bankName, idempotencyKey, cancellationToken);
    }
    public async Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceAccount>? accounts = await GetServiceAccounts(phoneNumber, cancellationToken);
        if (accounts is null)
        {
            return (new EntryResponseDto("There is no bank account linked to this phone number", UssdMessageType.EndSession), null);
        }
        var userAccounts = accounts.ToList();
        if (!userAccounts.Any()) { return (new EntryResponseDto("There is no bank account linked to this phone number", UssdMessageType.EndSession), null); }
        string responseMsg = string.Join("%0A", userAccounts.Select((item, index) => $"{index + 1}. {item.AccountNumber}"));
        return (new EntryResponseDto($"Please input the account you would like to use for this transaction%0A%0A{responseMsg}", UssdMessageType.ContinueSession), userAccounts);
    }
    public async Task SetIdempotencyKey() { }

}

