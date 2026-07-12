
public class FraudFlagRepository : IFraudFlagRepository
{
    private readonly MockDbContext _context;
    public FraudFlagRepository(MockDbContext context)
    {
        _context = context;
    }
    public async Task CreateFlagAsync(MockFraudFlag flag, CancellationToken cancellationToken) => await _context.MockFraudFlags.AddAsync(flag, cancellationToken);
    public void Update(MockFraudFlag flag) => _context.MockFraudFlags.Update(flag);
    public void Delete(MockFraudFlag flag) => _context.MockFraudFlags.Remove(flag);
    public async Task SaveChangesAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
}