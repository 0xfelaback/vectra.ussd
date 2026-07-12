using Microsoft.EntityFrameworkCore;

public class BvnReadRepository : IBvnReadRepository
{
    private readonly MockDbContext _context;

    public BvnReadRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<MockBvnRecord?> QueryBvnByBVN(string bvn, CancellationToken cancellationToken) =>
        await _context.MockBvnRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.BvnNumber == bvn, cancellationToken);

    public async Task<MockBvnRecord?> QueryBvnByPhoneNumber(string phone, CancellationToken token) =>
        await _context.MockBvnRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.PhoneNumber == phone, token);
}
