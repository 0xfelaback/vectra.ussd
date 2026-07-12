using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;
using Vectra.Ussd.Domain.Entities.CoreBanking;

public class DataRechargeService : IDataRechargeService
{
    private readonly IDataBundleRepository _dataBundleRepository;
    private readonly IDataPurchaseRepository _dataPurchaseRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<string> _passwordHasher;
    public DataRechargeService(IDataBundleRepository dataBundleRepository, ICustomerRepository customerRepository, IMapper mapper, IServiceAccountRepository serviceAccountRepository,
      IPasswordHasher<string> passwordHasher, IDataPurchaseRepository dataPurchaseRepository)
    {
        _dataBundleRepository = dataBundleRepository;
        _mapper = mapper; _serviceAccountRepository = serviceAccountRepository;
        _passwordHasher = passwordHasher;
        _customerRepository = customerRepository;
        _dataPurchaseRepository = dataPurchaseRepository;
    }

    public async Task<IEnumerable<DataBundleQueryDto>?> GetActiveBundlesforTelco(AirtimeRecharge.AirtimeNetwork network, CancellationToken token)
    {
        IEnumerable<DataBundle?> bundles = await _dataBundleRepository.GetActiveBundlesByTelcoAsync(network, token);
        if (bundles is null) return null;
        return _mapper.Map<IEnumerable<DataBundleQueryDto>>(bundles);
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
    public (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng)
    {
        if (pinStrng.Length != 4) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        bool input = int.TryParse(pinStrng, out int value);
        if (!input) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        string pinHash = _passwordHasher.HashPassword(string.Empty, pinStrng);
        return (null, pinHash);
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
    public async Task<string?> DataPurchase(string senderPhoneNumber, string beneficiaryPhoneNumber, string remitterAccountNumber,
     decimal amount, AirtimeRecharge.AirtimeNetwork network, AirtimeRecharge.TransactionStatus status,
      bool isSelfRecharge, CancellationToken cancellationToken)
    {
        bool isDebited = await DebitAccountAsync(remitterAccountNumber, amount, cancellationToken);
        if (!isDebited) return null;
        return await _dataPurchaseRepository.CreateDataPurchaseAsync(senderPhoneNumber, beneficiaryPhoneNumber, remitterAccountNumber, amount, network, status, isSelfRecharge, cancellationToken);
    }

    public async Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceAccount>? accounts = await _serviceAccountRepository.GetAllByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (accounts is null || !accounts.Any())
        {
            return (new EntryResponseDto("You do not have any active accounts linked to this phone number.", UssdMessageType.EndSession), null);
        }

        string accountList = string.Join("%0A", accounts.Select((a, i) => $"{i + 1}. {a.AccountNumber}"));
        string responseMsg = $"Select Account:%0A{accountList}";
        return (new EntryResponseDto(responseMsg, UssdMessageType.ContinueSession), accounts.ToList());
    }
}

