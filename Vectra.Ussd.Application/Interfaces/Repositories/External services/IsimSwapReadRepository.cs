namespace Vectra.Ussd.Application.Interfaces;

public interface IISimActionsRepository
{
    Task<bool> CheckSwap(string phoneNumber, CancellationToken cancellationToken);
    Task<bool> CheckReassigned(string phoneNumber, CancellationToken cancellationToken);
}