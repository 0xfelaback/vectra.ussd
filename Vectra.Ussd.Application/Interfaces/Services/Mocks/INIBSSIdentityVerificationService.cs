namespace Vectra.Ussd.Application.Interfaces;

public interface INIBSSIdentityVerificationService
{
    Task<bool?> MatchValidation(DateOnly inputDate, CancellationToken token, string phoneNumber);
    Task<bool> IsSimSwapped(string phoneNumber, CancellationToken token);
    Task<bool> IsSimReassigned(string phoneNumber, CancellationToken token);
}