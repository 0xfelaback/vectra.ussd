using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly MockDbContext _context;
    public CustomerRepository(MockDbContext context)
    {
        _context = context;
    }
    public async Task<int> CreateUSSDPin1ForCustomer(string phoneNumber, string PinHash, CancellationToken cancellationToken)
    {
        return await _context.Customers.Where(a => a.PhoneNumber == phoneNumber).ExecuteUpdateAsync(s => s.SetProperty(b => b.ussdPin1Hash, PinHash), cancellationToken);
    }
    public async Task CreateUSSDPin2ForCustomer(string phoneNumber, string PinHash, CancellationToken cancellationToken)
    {
        await _context.Customers.Where(a => a.PhoneNumber == phoneNumber).ExecuteUpdateAsync(s => s.SetProperty(b => b.ussdPin2Hash, PinHash), cancellationToken);
    }
    public async Task EditUSSDPin1ForCustomer(string phoneNumber, string PinHash, CancellationToken cancellationToken)
    {
        await _context.Customers.Where(a => a.PhoneNumber == phoneNumber).ExecuteUpdateAsync(s => s.SetProperty(b => b.ussdPin1Hash, PinHash), cancellationToken);
    }
    public async Task EditUSSDPin2ForCustomer(string phoneNumber, string PinHash, CancellationToken cancellationToken)
    {
        await _context.Customers.Where(a => a.PhoneNumber == phoneNumber).ExecuteUpdateAsync(s => s.SetProperty(b => b.ussdPin2Hash, PinHash), cancellationToken);
    }
    public async Task RegisterCustomerForUssd(string phoneNumber, CancellationToken cancellationToken)
    {
        await _context.Customers.Where(a => a.PhoneNumber == phoneNumber).ExecuteUpdateAsync(s => s.SetProperty(b => b.IsUssdRegistered, true), cancellationToken);
    }
    public async Task IncrementPinTrial(string phoneNumber, CancellationToken cancellationToken)
    {

        await _context.Customers.Where(a => a.PhoneNumber == phoneNumber).ExecuteUpdateAsync(s => s.SetProperty(b => b.PinTrials, b => b.PinTrials + 1));
    }
    public async Task ResetPinTrial(string phoneNumber, CancellationToken cancellationToken)
    {

        await _context.Customers.Where(a => a.PhoneNumber == phoneNumber).ExecuteUpdateAsync(s => s.SetProperty(b => b.PinTrials, 0), cancellationToken);
    }
    public async Task<Customer?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Customers.FindAsync([id], cancellationToken);
    }
    public async Task<IEnumerable<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken)
    {
        return await _context.Customers.AsNoTracking().ToListAsync(cancellationToken);
    }
    public async Task<Customer?> GetCustomerByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);
    }
    public async Task<Customer?> GetCustomerByBvnAsync(string bvn, CancellationToken cancellationToken)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.BvnNumber == bvn, cancellationToken);
    }
    public async Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<bool> DeleteCustomerAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }
}