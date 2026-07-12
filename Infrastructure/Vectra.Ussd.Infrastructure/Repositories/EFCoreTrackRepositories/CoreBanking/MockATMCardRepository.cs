using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class MockATMCardRepository : IMockATMCardRepository
{
    private readonly MockDbContext _context;

    public MockATMCardRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task CreateCardAsync(MockATMCard card, CancellationToken cancellationToken) =>
        await _context.MockATMCards.AddAsync(card, cancellationToken);

    public async Task<MockATMCard?> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken) =>
        await _context.MockATMCards
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber, cancellationToken);

    public async Task<IEnumerable<MockATMCard>> GetByCustomerPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken) =>
        await _context.MockATMCards
            .AsNoTracking()
            .Where(c => c.CustomerAccount.PhoneNumber == phoneNumber)
            .ToListAsync(cancellationToken);
    public async Task ActivateATMCard(string cardNumber, CancellationToken cancellationToken)
    {
        await _context.MockATMCards.Where(c => c.CardNumber == cardNumber).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActivated, true), cancellationToken);
    }
    public async Task ChangePINAsync(string cardNumber, string newPINHash, CancellationToken token) =>
        await _context.MockATMCards.Where(c => c.CardNumber == cardNumber).ExecuteUpdateAsync(s => s.SetProperty(x => x.cardPINHash, newPINHash), token);

    public async Task EnablePOS(string cardNumber, CancellationToken cancellationToken) => await _context.MockATMCards.Where(a => a.CardNumber == cardNumber).ExecuteUpdateAsync(s => s.SetProperty(x => x.PosEnabled, true), cancellationToken);
    public async Task EnableATM(string cardNumber, CancellationToken cancellationToken) => await _context.MockATMCards.Where(a => a.CardNumber == cardNumber).ExecuteUpdateAsync(s => s.SetProperty(x => x.AtmEnabled, true), cancellationToken);
    public async Task EnableWEB(string cardNumber, CancellationToken cancellationToken) => await _context.MockATMCards.Where(a => a.CardNumber == cardNumber).ExecuteUpdateAsync(s => s.SetProperty(x => x.WebEnabled, true), cancellationToken);


    public async Task DisablePOS(string cardNumber, CancellationToken cancellationToken) => await _context.MockATMCards.Where(a => a.CardNumber == cardNumber).ExecuteUpdateAsync(s => s.SetProperty(x => x.PosEnabled, false), cancellationToken);
    public async Task DisableATM(string cardNumber, CancellationToken cancellationToken) => await _context.MockATMCards.Where(a => a.CardNumber == cardNumber).ExecuteUpdateAsync(s => s.SetProperty(x => x.AtmEnabled, false), cancellationToken);
    public async Task DisableWEB(string cardNumber, CancellationToken cancellationToken) => await _context.MockATMCards.Where(a => a.CardNumber == cardNumber).ExecuteUpdateAsync(s => s.SetProperty(x => x.WebEnabled, false), cancellationToken);


    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await _context.SaveChangesAsync(cancellationToken);
}
