public interface ICustomerRepository
{
    Task<int> CreateUSSDPin1ForCustomer(string phoneNumber, string PinHash, CancellationToken cancellationToken);
    Task CreateUSSDPin2ForCustomer(string phoneNumber, string PinHash, CancellationToken cancellationToken);
    Task EditUSSDPin1ForCustomer(string phoneNumber, string PinHash, CancellationToken cancellationToken);
    Task EditUSSDPin2ForCustomer(string phoneNumber, string PinHash, CancellationToken cancellationToken);
    Task RegisterCustomerForUssd(string phoneNumber, CancellationToken cancellationToken);
    Task IncrementPinTrial(string accountNumber, CancellationToken cancellationToken);
    Task ResetPinTrial(string accountNumber, CancellationToken cancellationToken);
    Task<Customer?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken);
    Task<Customer?> GetCustomerByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<Customer?> GetCustomerByBvnAsync(string bvn, CancellationToken cancellationToken);
    Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken);
    Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken);
    Task<bool> DeleteCustomerAsync(string phoneNumber, CancellationToken cancellationToken);
}