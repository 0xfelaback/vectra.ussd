using Microsoft.AspNetCore.Identity;

public sealed class ServiceAccountNumbersService : IServiceAccountNumbersService
{
    private readonly ICustomerAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IHandleSession _handleSession;
    private readonly IServiceAccountRepository _serviceAccountRepository;

    private readonly IPasswordHasher<string> _passwordHasher;

    public ServiceAccountNumbersService(
        ICustomerAccountReadRepository accountReadRepository, ICustomerAccountRepository customerAccountRepository,
        ICustomerAccountRepository accountRepository, ICustomerRepository customerRepository,
        IHandleSession handleSession, IServiceAccountRepository serviceAccountRepository,
        IPasswordHasher<string> passwordHasher)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _serviceAccountRepository = serviceAccountRepository;
        _handleSession = handleSession;
        _passwordHasher = passwordHasher;
    }

    public async Task<PasswordVerificationResult> ValidatePinAsync(string phoneNumber, string enteredPin, CancellationToken cancellationToken)
    {
        Customer? customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (customer is null) return PasswordVerificationResult.Failed;

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(string.Empty, customer.ussdPin1Hash ?? "", enteredPin);
        return result;
    }

    public async Task<string?> GetAccountNumbersByPhoneAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (account is null) return null;
        return account.AccountNumber;
    }

    public async Task<bool> DebitAccountAsync(string accountNumber, decimal amount, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);
        if (account is null) return false;
        if (account.CustomerAccount.Balance < amount) return false;

        account.CustomerAccount.Balance -= amount;
        await _accountRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<EntryResponseDto?> VerifyUserPINWithTrials<T>(T session, int maxPinTrials, string phoneNumber, string enteredPin,
     CancellationToken cancellationToken) where T : ServiceAccountNumbersSession
    {
        PasswordVerificationResult pinVerification = await ValidatePinAsync(phoneNumber, enteredPin, cancellationToken);
        if (pinVerification == PasswordVerificationResult.Failed)
        {
            session.pinTrials++;
            if (session.pinTrials >= maxPinTrials)
            {
                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                return new EntryResponseDto($"You have exceeded maximum PIN attempts. Please try again later.", UssdMessageType.EndSession);
            }
            int remainingTrials = maxPinTrials - session.pinTrials;
            await _handleSession.SaveSessionAsync(session, cancellationToken);
            return new EntryResponseDto($"Incorrect PIN. You have {remainingTrials} attempt remaining.", UssdMessageType.ContinueSession);
        }
        session.isPinValidated = true;
        session.CurrentStep++;
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return null;
    }

    public async Task<(EntryResponseDto? errorMsg, string? userAccountNumber)> DebitCustomer<T>(T session, string phoneNumber, decimal queryAmount, CancellationToken cancellationToken) where T : ServiceAccountNumbersSession
    {
        string? accountNumber = await GetAccountNumbersByPhoneAsync(phoneNumber, cancellationToken);
        if (accountNumber is null)
        {
            await _handleSession.RemoveSessionAsync(session, cancellationToken);
            return (new EntryResponseDto("No account numbers found for this phone number.", UssdMessageType.EndSession), null);
        }

        bool debited = await DebitAccountAsync(accountNumber, queryAmount, cancellationToken);
        if (!debited)
        {
            await _handleSession.RemoveSessionAsync(session, cancellationToken);
            return (new EntryResponseDto($"Insufficient balance to process this request. Query fee: ₦{queryAmount}.", UssdMessageType.EndSession), null);
        }
        return (null, accountNumber);
    }

    public async Task<IEnumerable<ServiceAccount>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceAccount?> accounts = await _serviceAccountRepository.GetAccountsByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (!accounts.Any()) return null;
        return accounts!;
    }
    public async Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceAccount>? accounts = await GetServiceAccounts(phoneNumber, cancellationToken);
        if (accounts is null)
        {
            return (new EntryResponseDto("There is no bank account linked to this phone number", UssdMessageType.ContinueSession), null);
        }
        var userAccounts = accounts.ToList();
        if (!userAccounts.Any()) { return (new EntryResponseDto("There is no bank account linked to this phone number", UssdMessageType.ContinueSession), null); }
        string responseMsg = string.Join("%0A", userAccounts.Select((item, index) => $"{index + 1}. {item.AccountNumber}"));
        return (new EntryResponseDto($"Please input the account you would like to use for this transaction%0A%0A{responseMsg}", UssdMessageType.ContinueSession), userAccounts);
    }


}
