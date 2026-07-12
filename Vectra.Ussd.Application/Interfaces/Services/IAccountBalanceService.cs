public interface IAccountBalanceService
{
    Task<decimal?> GetAccountBalance(string phoneNumber, CancellationToken cancellationToken);
    Task<(EntryResponseDto reponseDto, List<ServiceAccount>? userAccounts)> DisplayServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
    Task<IEnumerable<AccountQueryDto>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
}