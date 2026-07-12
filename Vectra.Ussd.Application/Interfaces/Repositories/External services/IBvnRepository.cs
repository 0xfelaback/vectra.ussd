
public interface IBvnReadRepository
{
    Task<MockBvnRecord?> QueryBvnByBVN(string bvn, CancellationToken token);
    Task<MockBvnRecord?> QueryBvnByPhoneNumber(string phoneNumber, CancellationToken token);
}
