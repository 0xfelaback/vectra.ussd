using Microsoft.AspNetCore.Identity;

public sealed class BVNInquiryService : IBVNInquiryService
{
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly IHandleSession _handleSession;
    public BVNInquiryService(
     IPasswordHasher<string> passwordHasher, IHandleSession handleSession, ICustomerRepository customerRepository, IServiceAccountRepository serviceAccountRepository)
    {
        _serviceAccountRepository = serviceAccountRepository;
        _customerRepository = customerRepository;
        _passwordHasher = passwordHasher;
        _handleSession = handleSession;
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
        await _serviceAccountRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
    public async Task<PasswordVerificationResult> ValidatePinAsync(string phoneNumber, string enteredPin,
     CancellationToken cancellationToken)
    {
        Customer? customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (customer is null) return PasswordVerificationResult.Failed;

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(string.Empty, customer.ussdPin1Hash ?? "", enteredPin);
        return result;
    }
    public async Task<EntryResponseDto?> VerifyUserPINWithTrials<T>(T session, int maxPinTrials, string phoneNumber, string enteredPin,
     CancellationToken cancellationToken) where T : BVNInquirySession
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

    public async Task<(EntryResponseDto? errorMsg, string? userAccountNumber)> DebitCustomer<T>(T session, string phoneNumber,
     decimal queryAmount, CancellationToken cancellationToken) where T : BVNInquirySession
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
}

