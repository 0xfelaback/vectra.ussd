public interface ICamuService
{
    Task<bool> NotifyAccountCreatedAsync(string imageurl, string? signatureurl);
}