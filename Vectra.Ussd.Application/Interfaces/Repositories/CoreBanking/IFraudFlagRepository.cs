public interface IFraudFlagRepository
{
    Task CreateFlagAsync(MockFraudFlag flag, CancellationToken cancellationToken);
    void Update(MockFraudFlag flag);
    void Delete(MockFraudFlag flag);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}