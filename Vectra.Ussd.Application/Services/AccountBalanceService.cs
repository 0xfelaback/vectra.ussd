using AutoMapper;

public class AccountBalanceService : IAccountBalanceService
{
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IMapper _mapper;
    public AccountBalanceService(IServiceAccountRepository serviceAccountRepository, IMapper mapper)
    {
        _serviceAccountRepository = serviceAccountRepository;
        _mapper = mapper;
    }
    public async Task<decimal?> GetAccountBalance(string accountNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);
        if (account is null) return null;
        return account.CustomerAccount.Balance;
    }
    public async Task<IEnumerable<AccountQueryDto>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceAccount?> accounts = await _serviceAccountRepository.GetAccountsByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (!accounts.Any()) return null;
        return _mapper.Map<IEnumerable<AccountQueryDto?>>(accounts)!;
    }
    public async Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<AccountQueryDto>? accounts = await GetServiceAccounts(phoneNumber, cancellationToken);
        if (accounts is null)
        {
            return (new EntryResponseDto("There is no bank account linked to this phone number", UssdMessageType.ContinueSession), null);
        }
        var userAccounts = accounts.ToList();
        if (!userAccounts.Any()) { return (new EntryResponseDto("There is no bank account linked to this phone number", UssdMessageType.ContinueSession), null); }
        string responseMsg = string.Join("%0A", userAccounts.Select((item, index) => $"{index + 1}. {item.accountNumber}"));
        return (new EntryResponseDto($"Please input the account you would like to use for this transaction%0A%0A{responseMsg}", UssdMessageType.ContinueSession), _mapper.Map<List<ServiceAccount>>(userAccounts));
    }
}