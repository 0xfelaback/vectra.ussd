public class CoreAccountValidationService : ICoreAccountValidationService
{
    private readonly ICustomerAccountRepository _customerAccountRepository;
    public CoreAccountValidationService(ICustomerAccountRepository customerAccountRepository)
    {
        _customerAccountRepository = customerAccountRepository;
    }
    public async Task<(EntryResponseDto? errorResonse, CustomerAccount? userAccount)> AccountValidation(string phoneNumber, string accountNumber, CancellationToken cancellationToken)
    {
        CustomerAccount? account = await _customerAccountRepository.CustomerHasAccount(accountNumber, phoneNumber, cancellationToken);
        if (account is null)
        {
            return (new EntryResponseDto("The specified account number is not linked to this phone number.", UssdMessageType.EndSession), null);
        }
        if (account.PhoneNumber != phoneNumber) new EntryResponseDto("The specified account does not belong to this customer", UssdMessageType.EndSession);
        if (account.Status == CustomerAccount.AccountStatus.Pnd || account.Status == CustomerAccount.AccountStatus.Dormant) return (new EntryResponseDto("This account is currently banned from making outgoing transfers.", UssdMessageType.EndSession), null);
        if (account.Status == CustomerAccount.AccountStatus.Removed) return (new EntryResponseDto("This account has been deactivated.", UssdMessageType.EndSession), null);
        return (null, account);
    }
}

