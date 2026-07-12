public class CamuService : ICamuService
{
    public Task<bool> NotifyAccountCreatedAsync(string imageurl, string? signatureurl)
    {
        return Task.FromResult(true);
    }
}

