public interface IMockATMCardRepository
{
    Task CreateCardAsync(MockATMCard card, CancellationToken cancellationToken);
    Task<MockATMCard?> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken);
    Task<IEnumerable<MockATMCard>> GetByCustomerPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    Task ActivateATMCard(string cardNumber, CancellationToken cancellationToken);
    Task ChangePINAsync(string cardNumber, string newPINHash, CancellationToken token);
    Task EnablePOS(string cardNumber, CancellationToken cancellationToken);
    Task EnableATM(string cardNumber, CancellationToken cancellationToken);
    Task EnableWEB(string cardNumber, CancellationToken cancellationToken);
    Task DisablePOS(string cardNumber, CancellationToken cancellationToken);
    Task DisableWEB(string cardNumber, CancellationToken cancellationToken);
    Task DisableATM(string cardNumber, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
