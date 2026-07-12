public interface INubanRepository
{
    Task<NubanInterbankAccounts?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);
    Task<IEnumerable<NubanInterbankAccounts>> GetByBankCodeAsync(string bankCode, CancellationToken cancellationToken);
    Task CreateNubanAsync(NubanInterbankAccounts nuban, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}