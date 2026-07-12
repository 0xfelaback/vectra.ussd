using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;

public sealed class AirtimeService : IAirtimeService
{
    private readonly IAirtimeRechargeRepository _airtimeRechargeRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<string> _passwordHasher;
    public AirtimeService(IAirtimeRechargeRepository airtimeRechargeRepository, ICustomerRepository customerRepository,
      IServiceAccountRepository serviceAccountRepository, IMapper mapper, IPasswordHasher<string> passwordHasher)
    {
        _airtimeRechargeRepository = airtimeRechargeRepository;
        _serviceAccountRepository = serviceAccountRepository;
        _passwordHasher = passwordHasher;
        _customerRepository = customerRepository;
        _mapper = mapper;
    }
    public async Task<AccountQueryDto?> ServiceAccountValidation(string phoneNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (account is null) return null;
        return _mapper.Map<AccountQueryDto>(account);
    }
    public async Task<bool?> VerifyPin1(string providedPin1Hash, string phoneNumber, CancellationToken cancellationToken)
    {
        Customer? customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (customer is null) return null;
        PasswordVerificationResult varResult = _passwordHasher.VerifyHashedPassword(string.Empty, customer.ussdPin1Hash ?? "", providedPin1Hash);
        if (varResult == PasswordVerificationResult.Failed) return false;
        if (customer.ussdPin1Hash != providedPin1Hash) return false;
        return true;
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
    public async Task<string?> CreateAirtimeTransaction(string senderPhoneNumber, string beneficiaryPhoneNumber, string remitterAccountNumber,
     decimal amount, AirtimeRecharge.AirtimeNetwork network, AirtimeRecharge.TransactionStatus status,
      bool isSelfRecharge, CancellationToken cancellationToken)
    {
        bool isDebited = await DebitAccountAsync(remitterAccountNumber, amount, cancellationToken);
        if (!isDebited) return null;
        return await _airtimeRechargeRepository.CreateAirtimeRechargeAsync(senderPhoneNumber, beneficiaryPhoneNumber,
         remitterAccountNumber, amount, network, status, isSelfRecharge, cancellationToken);
        //debit user
    }
    public (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng)
    {
        if (pinStrng.Length != 4) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        bool input = int.TryParse(pinStrng, out int value);
        if (!input) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        string pinHash = _passwordHasher.HashPassword(string.Empty, pinStrng);
        return (null, pinHash);
    }
    public (EntryResponseDto? errorResponse, string? filteredPhoneNumber) ValidateBeneficiaryPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return (new EntryResponseDto("Please enter the beneficiary phone number", UssdMessageType.ContinueSession), null);
        string filteredInput = phoneNumber.Trim().Replace("+", "").Replace(" ", "");
        if (!filteredInput.All(char.IsDigit)) return (new EntryResponseDto("Please enter a valid phone number", UssdMessageType.ContinueSession), null);
        if (filteredInput.Length != 11 || !filteredInput.StartsWith('0'))
            return (new EntryResponseDto("Please enter a valid 11 digit Nigerian phone number", UssdMessageType.ContinueSession), null);
        return (null, filteredInput);
    }

    public async Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceAccount>? accounts = await _serviceAccountRepository.GetAllByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (accounts is null || !accounts.Any())
        {
            return (new EntryResponseDto("You do not have any active accounts linked to this phone number.", UssdMessageType.EndSession), null);
        }

        string accountList = string.Join("%0A", accounts.Select((item, index) => $"{index + 1}. {item.AccountNumber}"));
        string responseMsg = $"Select Account:%0A{accountList}";
        return (new EntryResponseDto(responseMsg, UssdMessageType.ContinueSession), accounts.ToList());
    }
}