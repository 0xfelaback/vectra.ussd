using AutoMapper;
using Microsoft.AspNetCore.Identity;

public sealed class RegistrationService : IRegistrationService
{
    private readonly IBvnReadRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly IMapper _mapper;
    private readonly IHandleSession _handleSession;
    public RegistrationService(IPasswordHasher<string> passwordHasher, IHandleSession handlesession, ICustomerRepository customerRepository, IServiceAccountRepository serviceAccountRepository,
     IBvnReadRepository repository, IMapper mapper)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _serviceAccountRepository = serviceAccountRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _handleSession = handlesession;
    }
    public async Task<AccountQueryDto?> ServiceAccountValidation(string phoneNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (account is null) return null;
        return _mapper.Map<AccountQueryDto>(account);
    }
    public async Task<AccountQueryDto?> AccountNumberValidation(string accountNumber, CancellationToken cancellationToken)
    {
        ServiceAccount? account = await _serviceAccountRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);
        if (account is null) return null;
        return _mapper.Map<AccountQueryDto>(account);
    }
    public async Task<BvnQueryDto?> BvnValidation(string bvnNumber, CancellationToken token)
    {
        MockBvnRecord? bvnRecord = await _repository.QueryBvnByBVN(bvnNumber, token);
        if (bvnRecord is null) return null;
        return _mapper.Map<BvnQueryDto>(bvnRecord);
    }
    public async Task<int> CreateUSSDPin1(string accountNumber, string PinHash, CancellationToken cancellationToken)
    {
        int res = await _customerRepository.CreateUSSDPin1ForCustomer(accountNumber, PinHash, cancellationToken);
        await _serviceAccountRepository.SaveChangesAsync(cancellationToken);
        return res;
    }

    public async Task<CardQueryDto?> GetOneCardFromAccount(string accountNumber, CancellationToken cancellationToken)
    {
        MockATMCard? card = await _serviceAccountRepository.GetSingleActiveATMCard(accountNumber, cancellationToken);
        if (card is null) return null;
        return _mapper.Map<CardQueryDto>(card);
    }

    public async Task RegisterUssd(string accountNumber, CancellationToken cancellationToken)
    {
        await _customerRepository.RegisterCustomerForUssd(accountNumber, cancellationToken);
        await _serviceAccountRepository.SaveChangesAsync(cancellationToken);
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
    public (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng)
    {
        if (pinStrng.Length != 4) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        bool input = int.TryParse(pinStrng, out int value);
        if (!input) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        string pinHash = _passwordHasher.HashPassword(string.Empty, pinStrng);
        return (null, pinHash);
    }
    public async Task<EntryResponseDto?> VerifyPinConfirmation<T>(T session, string ussdPinHash, string providedPassword, CancellationToken cancellationToken) where T : SessionBase
    {
        if (ussdPinHash == null) return new EntryResponseDto("PIN couldn't be processed, the process failed.%0ATry again later", UssdMessageType.EndSession);
        PasswordVerificationResult res = _passwordHasher.VerifyHashedPassword(string.Empty, ussdPinHash, providedPassword);
        if (res == PasswordVerificationResult.Failed) return new EntryResponseDto("The PIN you entered don't match. Please try again", UssdMessageType.ContinueSession);

        int result = await CreateUSSDPin1(session.accountNumber, ussdPinHash, cancellationToken);
        if (result < 1)
        {
            await _handleSession.RemoveSessionAsync(session, cancellationToken);
            return new EntryResponseDto("An error ocured creating the USSD PIN.%0APlease try again later", UssdMessageType.EndSession);
        }

        await RegisterUssd(session.accountNumber, cancellationToken);

        return null;
    }

    public async Task LinkInitialAccount(string phoneNumber, string accountNumber, string pinHash, CancellationToken cancellationToken)
    {
        await RegisterUssd(accountNumber, cancellationToken);
    }
}

