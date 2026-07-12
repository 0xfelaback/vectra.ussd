public interface ICustomerAccountReadRepository
{
    Task<CustomerAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);
    Task<CustomerAccount?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);

}
