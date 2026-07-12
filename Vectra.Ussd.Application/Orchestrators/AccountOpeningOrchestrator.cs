using Microsoft.Extensions.Caching.Distributed;

public sealed class AccountOpeningOrchestrator : IAccountOpeningOrchestrator
{
    private readonly IDistributedCache _cache;
    private readonly IAccountOpeningService _service;
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly IVerfySimChecks _verfySimChecks;
    private const string _unrecognisedMsg = "Unrecognised request. Please try again by selecting a valid option";
    public AccountOpeningOrchestrator(IAccountOpeningService service, IDistributedCache cache, IHandleSession handlesession, IVerifySession verifySession, IVerfySimChecks verfySimChecks)
    {
        _cache = cache;
        _service = service;
        _handleSession = handlesession;
        _verifySession = verifySession;
        _verfySimChecks = verfySimChecks;
    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        AccountOpeningSession? session = new AccountOpeningSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.accountOpening
        };
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return new EntryResponseDto("Please enter your BVN", UssdMessageType.ContinueSession);
    }
    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var verifyResult = await _verifySession.VerifySessionAsync<AccountOpeningSession>(sessionBase, cancellationToken);
        if (verifyResult.error != null)
        {
            return verifyResult.error;
        }
        AccountOpeningSession? session = verifyResult.session;

        switch (session!.CurrentStep)
        {
            case 1:
                string responseMsg;
                //bvn validation
                BvnQueryDto? result = await _service.BvnValidation(requestDto.msg, cancellationToken);
                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                if (result is null) return new EntryResponseDto("The entered BVN number does not exist", UssdMessageType.EndSession);
                session.BVN = result.bvnNumber;
                session.DOB = result.DateOfBirth;
                session.isBvnActive = result.IsActive;
                session.FirstName = result.FirstName;
                session.LastName = result.LastName;
                session.ImageUrl = result.ImageUrl;
                session.SignatureUrl = result.SignatureUrl;
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);

                bool isMatch = result.PhoneNumber == session.PhoneNumber;
                responseMsg = isMatch ? "Please enter your Date of Birth (DD-MM-YYYY)" : "Entered BVN does not match the phone number initiating this request. Please use the phone number attached to your BVN";
                return new EntryResponseDto(responseMsg, isMatch ? UssdMessageType.ContinueSession : UssdMessageType.EndSession);

            case 2:
                DateOnly.TryParse(requestDto.msg, out DateOnly resultDate);
                if (resultDate != session.DOB)
                {
                    string responsemsg = "Please specify the Date of Birth attatched to the BVN";
                    return new EntryResponseDto(responsemsg, UssdMessageType.EndSession);
                }
                EntryResponseDto? simVerifyresults = await _verfySimChecks.VerifySimSwapandReassign(session, cancellationToken);
                if (simVerifyresults != null) return simVerifyresults;
                if (!session.isBvnActive)
                {
                    responseMsg = "Operation Failed: BVN number has been deactivated";
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto(responseMsg, UssdMessageType.EndSession);
                }
                session.BvnValidated = true;
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                responseMsg = $"FullName: {session.FirstName} {session.LastName}, %0ADOB: {DateOnly.Parse(session.DOB.Value.ToString())}, %0APhone: {session.PhoneNumber}. %0A%0AReply 1 to confirm, 2 to cancel.";
                return new EntryResponseDto(responseMsg, UssdMessageType.ContinueSession);

            case 3:
                int.TryParse(requestDto.msg, out int inputResult);
                switch (inputResult)
                {
                    case 1:
                        session.CurrentStep++;
                        string? accountNumber = await _service.CreateAccountAsync(session.PhoneNumber, session.BVN, session.ImageUrl, cancellationToken, session.SignatureUrl);
                        if (accountNumber is null) return new EntryResponseDto("Customer record does not exist on bank records", UssdMessageType.EndSession);
                        responseMsg = $"Account opened successfully.%0AYour account number is {accountNumber}";
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto(responseMsg, UssdMessageType.EndSession);
                    case 2:
                        session.CurrentStep++;
                        responseMsg = "Account opening cancelled.";
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto(responseMsg, UssdMessageType.EndSession);

                    default:
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
                }
            default:
                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
        }
        ;
    }
}


