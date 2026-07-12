using Vectra.Ussd.Application.Interfaces;

public class VerfySimChecks : IVerfySimChecks
{
    private readonly IHandleSession _handleSession;
    private readonly INIBSSIdentityVerificationService _nibssService;
    private readonly IFraudFlagRepository _flagrepository;

    public VerfySimChecks(IHandleSession handlesession, INIBSSIdentityVerificationService nibssService, IFraudFlagRepository flagrepository)
    {
        _handleSession = handlesession;
        _nibssService = nibssService;
        _flagrepository = flagrepository;
    }
    public async Task<EntryResponseDto?> VerifySimSwapandReassign(RegistrationAndAccountOpeningSessionBase session, CancellationToken cancellationToken)
    {
        bool IsSimSwapped = await SimSwap(session.PhoneNumber, cancellationToken);
        session.SimSwapChecked = true;
        bool IsSimReassigned = await SimResaasigned(session.PhoneNumber, cancellationToken);
        session.SimReassignedChecked = true;
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        if (IsSimSwapped || IsSimReassigned)
        {
            string? responseMsg;
            switch (IsSimSwapped, IsSimReassigned)
            {
                case (true, true):
                    await NotifyFraudDesk(session.PhoneNumber, session.BVN, IsSimSwapped, IsSimReassigned, cancellationToken);
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    responseMsg = "Operation Failed: MSISDN has been swapped and reassigned%0AYour request has been flagged for further review. Please visit a branch";
                    return new EntryResponseDto(responseMsg, UssdMessageType.EndSession);
                case (true, false):
                    await NotifyFraudDesk(session.PhoneNumber, session.BVN, IsSimSwapped, IsSimReassigned, cancellationToken);
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    responseMsg = "Operation Failed: MSISDN has been swapped%0AYour request has been flagged for further review. Please visit a branch";
                    return new EntryResponseDto(responseMsg, UssdMessageType.EndSession);
                case (false, true):
                    await NotifyFraudDesk(session.PhoneNumber, session.BVN, IsSimSwapped, IsSimReassigned, cancellationToken);
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    responseMsg = "Operation Failed: MSISDN has been reassigned%0AYour request has been flagged for further review. Please visit a branch";
                    return new EntryResponseDto(responseMsg, UssdMessageType.EndSession);
                default: throw new ArgumentOutOfRangeException();
            }
        }
        return null;
    }
    public async Task<bool> SimSwap(string phoneNumber, CancellationToken token)
    {
        return await _nibssService.IsSimReassigned(phoneNumber, token) ? true : false;
    }
    public async Task<bool> SimResaasigned(string phoneNumber, CancellationToken token)
    {
        return await _nibssService.IsSimReassigned(phoneNumber, token) ? true : false;
    }
    public async Task NotifyFraudDesk(string phoneNumber, string bvn, bool IsSimSwapped, bool IsSimReassigned, CancellationToken cancellationToken, string? accountNumber = null)
    {
        var reason = (IsSimSwapped, IsSimReassigned) switch
        {
            (true, true) => FlagReason.SwappedSimandReassignedSim,
            (true, false) => FlagReason.SwappedSIM,
            (false, true) => FlagReason.ReassignedSIM,
            _ => throw new ArgumentOutOfRangeException()
        };
        MockFraudFlag flag = new MockFraudFlag
        {
            PhoneNumber = phoneNumber,
            BvnNumber = bvn,
            Reason = reason,
            AccountNumber = accountNumber
        };
        await _flagrepository.CreateFlagAsync(flag, cancellationToken);
        await _flagrepository.SaveChangesAsync(cancellationToken);
    }
}

public interface IVerfySimChecks
{
    Task<EntryResponseDto?> VerifySimSwapandReassign(RegistrationAndAccountOpeningSessionBase session, CancellationToken cancellationToken);
}