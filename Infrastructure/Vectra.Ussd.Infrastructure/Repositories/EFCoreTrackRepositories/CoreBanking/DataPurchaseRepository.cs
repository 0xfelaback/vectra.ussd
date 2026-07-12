public class DataPurchaseRepository : IDataPurchaseRepository
{
    private readonly MockDbContext _context;
    public DataPurchaseRepository(MockDbContext context)
    {
        _context = context;
    }
    public async Task<string?> CreateDataPurchaseAsync(string senderPhoneNumber, string beneficiaryPhoneNumber, string remitterAccountNumber, decimal amount, AirtimeRecharge.AirtimeNetwork network, AirtimeRecharge.TransactionStatus status,
      bool isSelfRecharge, CancellationToken cancellationToken)
    {
        AirtimeRecharge purchase = new AirtimeRecharge
        {
            SenderPhoneNumber = senderPhoneNumber,
            BeneficiaryPhoneNumber = beneficiaryPhoneNumber,
            RemitterAccountNumber = remitterAccountNumber,
            Amount = amount,
            Network = network,
            Status = status,
            IsSelfRecharge = isSelfRecharge,
            Channel = "DATA",
            TransactionReference = Guid.NewGuid().ToString("N"),
            AggregatorReference = Guid.NewGuid().ToString("N")
        };
        await _context.AirtimeRecharges.AddAsync(purchase, cancellationToken);
        return purchase.TransactionReference;
    }
}