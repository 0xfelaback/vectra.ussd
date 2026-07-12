public interface IAccountOpeningService
{
    Task<BvnQueryDto?> BvnValidation(string bvnNumber, CancellationToken token);
    Task<bool?> NibSSLookup(DateOnly inputDate, CancellationToken token, string phoneNumber = default!);
    Task<string?> CreateAccountAsync(string phoneNumber, string bvnNumber, string imageurl, CancellationToken cancellationToken, string? signatureurl = null);
}