using Vectra.Ussd.Application.Interfaces;
using AutoMapper;

public sealed class AccountOpeningService : IAccountOpeningService
{
    private readonly IBvnReadRepository _repository;
    private readonly INIBSSIdentityVerificationService _nibssService;
    private readonly ICustomerAccountRepository _customerAccountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IMapper _mapper;
    private readonly ICamuService _camu;

    public AccountOpeningService(IBvnReadRepository repository, INIBSSIdentityVerificationService nibssService, ICustomerRepository customerRepository,
    IServiceAccountRepository serviceAccountRepository,
     IMapper mapper, ICustomerAccountRepository customerAccountRepository, ICamuService camu)
    {
        _repository = repository;
        _nibssService = nibssService;
        _mapper = mapper;
        _customerAccountRepository = customerAccountRepository;
        _customerRepository = customerRepository;
        _serviceAccountRepository = serviceAccountRepository;
        _camu = camu;
    }

    public async Task<BvnQueryDto?> BvnValidation(string bvnNumber, CancellationToken token)
    {
        MockBvnRecord? bvnRecord = await _repository.QueryBvnByBVN(bvnNumber, token);
        if (bvnRecord is null) return null;
        return _mapper.Map<BvnQueryDto>(bvnRecord);
    }
    public async Task<bool?> NibSSLookup(DateOnly inputDate, CancellationToken token, string phoneNumber = default!)
    {
        bool? validationResult = await _nibssService.MatchValidation(inputDate, token, phoneNumber);
        bool? result = validationResult switch
        {
            null => null,
            false => false,
            true => true,
        };
        return result;
    }

    public async Task<string?> CreateAccountAsync(string phoneNumber, string bvnNumber, string imageurl, CancellationToken cancellationToken, string? signatureurl)
    {
        Customer? customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (customer is null) return null;
        var generatedAccountNumber = GenerateAccountNumber();
        // YOU NEVER LINK CUSTOMER ID
        var account = new CustomerAccount
        {
            CustomerId = customer.Id,
            AccountNumber = generatedAccountNumber,
            PhoneNumber = phoneNumber,
            BvnNumber = bvnNumber,
            IsUssdRegistered = true,
            ImageUrl = imageurl,
            IsLinked = true,
            SignatureUrl = signatureurl
        };

        CustomerAccount entity = await _customerAccountRepository.CreateAccountAsync(account, cancellationToken);
        await _customerAccountRepository.SaveChangesAsync(cancellationToken);
        await _camu.NotifyAccountCreatedAsync(account.ImageUrl, account.SignatureUrl);
        // ACCOUNT CREATION VIA USSD
        ServiceAccount serviceAccount = new ServiceAccount
        {
            CustomerId = entity.CustomerId,
            CustomerAccountId = entity.Id,
            PhoneNumber = entity.PhoneNumber,
            BvnNumber = entity.BvnNumber,
            AccountNumber = entity.AccountNumber,
            DateLinked = DateTime.Now,

        };
        await _serviceAccountRepository.CreateServiceAccountAsync(serviceAccount, cancellationToken);
        await _serviceAccountRepository.SaveChangesAsync(cancellationToken);

        return entity.AccountNumber;
    }

    private static string GenerateAccountNumber()
    {
        var random = new Random();
        return string.Concat(Enumerable.Range(0, 10).Select(_ => random.Next(0, 10).ToString()));
    }
}


