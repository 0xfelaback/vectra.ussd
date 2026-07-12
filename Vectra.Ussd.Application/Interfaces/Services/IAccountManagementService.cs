public interface IAccountManagementService
{
    Task<AccountQueryDto?> ServiceAccountValidation(string phoneNumber, CancellationToken cancellationToken);
    Task<IEnumerable<ServiceAccount>?> GetServiceAccounts(string phoneNumber, CancellationToken cancellationToken);
    Task<CustomrQueryDto?> GetCustomerDetails(string phoneNumber, CancellationToken cancellationToken);
    Task<ServiceAccount?> GetPrimaryAccount(string phoneNumber, CancellationToken cancellationToken);
    Task<(EntryResponseDto? errorResponse, bool isSuccess)> AddAccount(string phoneNumber, string accountNumber, CancellationToken cancellationToken);
    Task<(EntryResponseDto? errorResponse, bool isSuccess)> SetPrimaryAccount(string phoneNumber, string accountNumber, CancellationToken cancellationToken);
    Task<(EntryResponseDto? errorResponse, bool isSuccess)> RemovePrimaryAccount(string phoneNumber, string accountNumber, CancellationToken cancellationToken);
    Task<(EntryResponseDto? errorResponse, bool isSuccess)> RemoveAccount(string phoneNumber, string accountNumber, CancellationToken cancellationToken);
}