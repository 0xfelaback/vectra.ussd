using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;
using Vectra.Ussd.Domain.Entities.CoreBanking;

public sealed class CardRequestService : ICardRequestService
{
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly ITransactionHistoryRepository _transactionHistoryRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher<string> _passwordHasher;

    private readonly IMapper _mapper;
    public CardRequestService(IServiceAccountRepository serviceAccountRepository, IPasswordHasher<string> passwordHasher, ITransactionHistoryRepository transactionHistoryRepository, ICustomerRepository customerRepository, IMapper mapper)
    {
        _serviceAccountRepository = serviceAccountRepository;
        _transactionHistoryRepository = transactionHistoryRepository;
        _passwordHasher = passwordHasher;
        _customerRepository = customerRepository;
        _mapper = mapper;
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
    public async Task<(List<string> eligiblecards, string eligiblecardnetworks)> GetAvailableCardsforCustomer(int customerTierLevel, CustomerAccount.AccountType accountType, decimal mastercardBalanceThreshold, decimal customerAccountBalance, string accountNumber, CancellationToken cancellationToken)
    {
        List<string> eligibleCards = new List<string>();
        string eligibleCardNetworks;
        if (customerTierLevel == 1)
        {
            eligibleCards.Add("1. Verve");
            eligibleCardNetworks = "Verve";
        }
        else if (accountType == CustomerAccount.AccountType.Savings)
        {
            eligibleCards.Add("1. Verve");
            eligibleCardNetworks = "Verve";
        }
        else if (accountType == CustomerAccount.AccountType.Current)
        {
            eligibleCards.Add("1. Verve");
            bool meetsBalanceThreshold = customerAccountBalance >= mastercardBalanceThreshold;

            bool meetsNetTurnoverThreshold = false;
            var transactions = await _transactionHistoryRepository.GetAccountTransactionsAsync(
                accountNumber, null, null, cancellationToken);

            decimal netTurnover = transactions
                .Where(t => t.ReceiverAccountNumber == accountNumber && t.Status == TransactionHistory.TransactionStatus.Success)
                .Sum(t => t.Amount);
            meetsNetTurnoverThreshold = netTurnover >= mastercardBalanceThreshold;

            if (meetsBalanceThreshold || meetsNetTurnoverThreshold)
            {
                eligibleCards.Add("2. Standard Mastercard");
                eligibleCardNetworks = "Verve,MasterCard";
            }
            else
            {
                eligibleCardNetworks = "Verve";
            }
        }
        else
        {
            eligibleCards.Add("1. Verve");
            eligibleCardNetworks = "Verve";
        }
        return (eligibleCards, eligibleCardNetworks);
    }
    public (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng)
    {
        if (pinStrng.Length != 4) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        bool input = int.TryParse(pinStrng, out int value);
        if (!input) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        string pinHash = _passwordHasher.HashPassword(string.Empty, pinStrng);
        return (null, pinHash);
    }
    public async Task<bool> VerifyPin1(string providedPin1Hash, string phoneNumber, CancellationToken cancellationToken)
    {
        Customer? customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber, cancellationToken); PasswordVerificationResult varResult = _passwordHasher.VerifyHashedPassword(string.Empty, customer!.ussdPin1Hash ?? "", providedPin1Hash);
        if (varResult == PasswordVerificationResult.Failed) return false;
        if (customer.ussdPin1Hash != providedPin1Hash) return false;
        return true;
    }
}

public interface ICardRequestService
{
    Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
    Task<IEnumerable<AccountQueryDto>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
    Task<(List<string> eligiblecards, string eligiblecardnetworks)> GetAvailableCardsforCustomer(int customerTierLevel, CustomerAccount.AccountType accountType, decimal mastercardBalanceThreshold, decimal customerAccountBalance, string accountNumber, CancellationToken cancellationToken);
    (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng);
    Task<bool> VerifyPin1(string providedPin1Hash, string phoneNumber, CancellationToken cancellationToken);
}