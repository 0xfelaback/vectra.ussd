using AutoMapper;
using Microsoft.AspNetCore.Identity;

public sealed class PINManagementService : IPINManagementService
{
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IBvnReadRepository _bvnReadRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly IMapper _mapper;
    public PINManagementService(ICustomerRepository customerRepository,
      IServiceAccountRepository serviceAccountRepository, IPasswordHasher<string> passwordHasher,
     IMapper mapper, IBvnReadRepository bvnReadRepository)
    {
        _serviceAccountRepository = serviceAccountRepository;
        _bvnReadRepository = bvnReadRepository;
        _customerRepository = customerRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }
    public async Task<IEnumerable<AccountQueryDto>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceAccount?> accounts = await _serviceAccountRepository.GetAccountsByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (!accounts.Any()) return null;
        return _mapper.Map<IEnumerable<AccountQueryDto>>(accounts);
    }
    public async Task<BvnQueryDto?> BvnValidation(string bvnNumber, CancellationToken token)
    {
        MockBvnRecord? bvnRecord = await _bvnReadRepository.QueryBvnByBVN(bvnNumber, token);
        if (bvnRecord is null) return null;
        return _mapper.Map<BvnQueryDto>(bvnRecord);
    }
    public async Task<BvnQueryDto?> BvnValidationByPhoneNumber(string phoneNumber, CancellationToken token)
    {
        MockBvnRecord? bvnRecord = await _bvnReadRepository.QueryBvnByPhoneNumber(phoneNumber, token);
        if (bvnRecord is null) return null;
        return _mapper.Map<BvnQueryDto>(bvnRecord);
    }

    public async Task<CardQueryDto?> GetOneCardFromAccount(string accountNumber, CancellationToken cancellationToken)
    {
        MockATMCard? card = await _serviceAccountRepository.GetSingleActiveATMCard(accountNumber, cancellationToken);
        if (card is null) return null;
        return _mapper.Map<CardQueryDto>(card);
        //return card;
    }
    public async Task<bool> ValidateLastSixDigits(string sixDigits, string accountNumber, CancellationToken cancellationToken)
    {
        var cards = await _serviceAccountRepository.GetAllActiveATMCard(accountNumber, cancellationToken);
        if (cards is null || !cards.Any()) return false;

        return cards.Any(card =>
            !string.IsNullOrEmpty(card.CardNumber)
             && card.CardNumber.EndsWith(sixDigits)
        );
    }
    public async Task CreateUSSDPin1(string accountNumber, string PinHash, CancellationToken cancellationToken)
    {
        await _customerRepository.CreateUSSDPin1ForCustomer(accountNumber, PinHash, cancellationToken);
        await _serviceAccountRepository.SaveChangesAsync(cancellationToken);
    }
    public async Task CreateUSSDPin2(string accountNumber, string PinHash, CancellationToken cancellationToken)
    {
        await _customerRepository.CreateUSSDPin2ForCustomer(accountNumber, PinHash, cancellationToken);
        await _serviceAccountRepository.SaveChangesAsync(cancellationToken);
    }
    public async Task EditUSSDPin1(string accountNumber, string PinHash, CancellationToken cancellationToken)
    {
        await _customerRepository.EditUSSDPin1ForCustomer(accountNumber, PinHash, cancellationToken);
        await _serviceAccountRepository.SaveChangesAsync(cancellationToken);
    }
    public async Task EditUSSDPin2(string accountNumber, string PinHash, CancellationToken cancellationToken)
    {
        await _customerRepository.EditUSSDPin2ForCustomer(accountNumber, PinHash, cancellationToken);
        await _serviceAccountRepository.SaveChangesAsync(cancellationToken);
    }
    public async Task<bool> VerifyPin1(string providedPin1Hash, string phoneNumber, CancellationToken cancellationToken)
    {
        Customer? customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber, cancellationToken); PasswordVerificationResult varResult = _passwordHasher.VerifyHashedPassword(string.Empty, customer!.ussdPin1Hash ?? "", providedPin1Hash);
        if (varResult == PasswordVerificationResult.Failed) return false;
        return true;
    }
    public async Task<bool> VerifyPin2(string providedPin2Hash, string phoneNumber, CancellationToken cancellationToken)
    {
        Customer? customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber, cancellationToken); PasswordVerificationResult varResult = _passwordHasher.VerifyHashedPassword(string.Empty, customer!.ussdPin1Hash ?? "", providedPin2Hash);
        if (varResult == PasswordVerificationResult.Failed) return false;
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
    public EntryResponseDto? VerifyPinConfirmation(string? ussdPinHash, string providedPassword)
    {
        if (string.IsNullOrEmpty(ussdPinHash)) return new EntryResponseDto("PIN couldn't be processed, the process failed.%0ATry again later", UssdMessageType.EndSession);
        PasswordVerificationResult res = _passwordHasher.VerifyHashedPassword(string.Empty, ussdPinHash, providedPassword);
        if (res == PasswordVerificationResult.Failed) return new EntryResponseDto("The PIN you entered don't match. Please try again", UssdMessageType.ContinueSession);

        return null;
    }
}

