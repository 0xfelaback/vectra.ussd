using AutoMapper;

public sealed class AccountManagementService : IAccountManagementService
{
    private readonly ICustomerAccountRepository _customerAccountRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICoreAccountValidationService _coreAccountValidationService;
    private readonly IMapper _mapper;

    public AccountManagementService(IServiceAccountRepository serviceAccountRepository, ICustomerAccountRepository customerAccountRepository, ICustomerRepository customerRepository, ICoreAccountValidationService coreAccountValidationService, IMapper mapper)
    {
        _serviceAccountRepository = serviceAccountRepository;
        _customerAccountRepository = customerAccountRepository;
        _customerRepository = customerRepository;
        _coreAccountValidationService = coreAccountValidationService;
        _mapper = mapper;
    }
    public async Task<AccountQueryDto?> ServiceAccountValidation(string phoneNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (account is null) return null;
        return _mapper.Map<AccountQueryDto>(account);
    }
    public async Task<CustomrQueryDto?> GetCustomerDetails(string phoneNumber, CancellationToken cancellationToken)
    {
        Customer? customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (customer is null) return null;
        return _mapper.Map<CustomrQueryDto>(customer);
    }
    public async Task<IEnumerable<ServiceAccount>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceAccount?> accounts = await _serviceAccountRepository.GetAccountsByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (!accounts.Any()) return null;
        return accounts!;
    }
    public async Task<ServiceAccount?> GetPrimaryAccount(string phoneNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetCustomerPrimaryAccount(phoneNumber, cancellationToken);
        if (account is null) return null;
        return account;
    }
    public async Task<(EntryResponseDto? errorResponse, bool isSuccess)> AddAccount(string phoneNumber, string accountNumber, CancellationToken cancellationToken)
    {
        var customerDetails = await GetCustomerDetails(phoneNumber, cancellationToken);
        if (customerDetails is null) return (new EntryResponseDto("This customer does not exist.", UssdMessageType.EndSession), false);
        var accountValidationResult = await _coreAccountValidationService.AccountValidation(phoneNumber, accountNumber, cancellationToken);
        if (accountValidationResult.errorResonse != null) return (accountValidationResult.errorResonse, false);

        if (accountValidationResult.userAccount!.IsLinked) return (new EntryResponseDto("The specified account is already linked on USSD.", UssdMessageType.EndSession), false);

        int ServiceAccountsCount = await _serviceAccountRepository.CountAccountsByPhoneNumberAsync(phoneNumber, cancellationToken);
        ServiceAccount serviceAccount = new ServiceAccount
        {
            CustomerId = customerDetails.customerId,
            CustomerAccountId = accountValidationResult.userAccount.Id,
            PhoneNumber = phoneNumber,
            BvnNumber = accountValidationResult.userAccount.BvnNumber,
            AccountNumber = accountNumber,
            IsPrimary = ServiceAccountsCount == 0 ? true : false,
            DateLinked = DateTime.Now
        };
        var result = await _serviceAccountRepository.CreateAccountAsync(serviceAccount, cancellationToken);
        if (result is null)
        {
            return (new EntryResponseDto("The specified account could not be linked due to an encountered error.", UssdMessageType.EndSession), false);
        }
        await _customerAccountRepository.LinkCustomerAccount(accountValidationResult.userAccount.Id, cancellationToken);
        return (null, true);
    }
    public async Task<(EntryResponseDto? errorResponse, bool isSuccess)> SetPrimaryAccount(string phoneNumber, string accountNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.CustomerHasAccount(accountNumber, phoneNumber, cancellationToken);
        if (account is null) return (new EntryResponseDto("You cannot set an account as primary that hasn't been added on USSD first", UssdMessageType.EndSession), false);
        var result = await _serviceAccountRepository.SetCustomerPrimaryAccount(accountNumber, cancellationToken);
        if (result < 1) return (new EntryResponseDto("Account not updated an error occured", UssdMessageType.EndSession), false);
        return (null, true);
    }

    public async Task<(EntryResponseDto? errorResponse, bool isSuccess)> RemovePrimaryAccount(string phoneNumber, string accountNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.CustomerHasAccount(accountNumber, phoneNumber, cancellationToken);
        if (account is null) return (new EntryResponseDto("You cannot remove an account as primary that hasn't been added on USSD first", UssdMessageType.EndSession), false);
        var result = await _serviceAccountRepository.RemoveCustomerPrimaryAccount(accountNumber, cancellationToken);
        if (result < 1) return (new EntryResponseDto("Account not updated an error occured", UssdMessageType.EndSession), false);
        return (null, true);
    }
    public async Task<(EntryResponseDto? errorResponse, bool isSuccess)> RemoveAccount(string phoneNumber, string accountNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetCustomerPrimaryAccount(phoneNumber, cancellationToken);
        if (account is null) return (new EntryResponseDto("You cannot remove an account that hasn't been added on USSD first", UssdMessageType.EndSession), false);
        if (account.AccountNumber == accountNumber) return (new EntryResponseDto("You cannot remove an account that is currently the primary account. Set a different account as primary and try again.", UssdMessageType.EndSession), false);

        if (await _customerAccountRepository.CountAccountsByPhoneNumberAsync(phoneNumber, cancellationToken) < 2) return (new EntryResponseDto("Customer must always have at least one linked account", UssdMessageType.EndSession), false);
        var result = await _customerAccountRepository.RemoveCustomerAccount(accountNumber, cancellationToken);
        if (result < 1) return (new EntryResponseDto("Account not updated an error occured", UssdMessageType.EndSession), false);
        return (null, true);
    }
}


