using Microsoft.AspNetCore.Identity;

public sealed class RegistrationOrchestrator : IRegistrationOrchestrator
{
    /* USER DIALS *822# AND SELECTS AN OPTION THAT REQUIRES AUTHENTICATION E.G. TRANSFER, AIRTIME.
    USSD PROFILE AND PIN SETUP SHOULD BE TRUE OTHERWISE DIRECT TO REGISTRATION 
    
    "You are not registered for USSD. Please provide your account number to register."*/
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly IVerfySimChecks _verfySimChecks;

    private readonly IRegistrationService _registrationService;
    public RegistrationOrchestrator(IHandleSession handlesession, IVerifySession verifySession, IRegistrationService registrationService, IVerfySimChecks verfySimChecks)
    {
        _handleSession = handlesession;
        _verifySession = verifySession;
        _registrationService = registrationService;
        _verfySimChecks = verfySimChecks;

    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        RegistrationSession session = new RegistrationSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.registration
        };
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return new EntryResponseDto("Please enter your existing Sterling account number", UssdMessageType.ContinueSession);
    }

    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var result = await _verifySession.VerifySessionAsync<RegistrationSession>(sessionBase, cancellationToken);
        if (result.error != null)
        {
            return result.error;
        }
        RegistrationSession? session = result.session;

        switch (session!.CurrentStep)
        {
            case 1:
                var dtoResult = await _registrationService.AccountNumberValidation(requestDto.msg, cancellationToken);
                if (dtoResult is null)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("We couldn't find an account with that number. Please check the number and try again.", UssdMessageType.EndSession);
                }
                else
                {
                    session.accountNumber = requestDto.msg;
                }
                if (dtoResult.phoneNumber != requestDto.msisdn)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("The account number entered does not match the phone number for this request", UssdMessageType.EndSession);
                }
                EntryResponseDto? simVerifyresults = await _verfySimChecks.VerifySimSwapandReassign(session, cancellationToken);
                if (simVerifyresults != null) return simVerifyresults;
                session.CurrentStep++;
                CardQueryDto? userCard = await _registrationService.GetOneCardFromAccount(requestDto.msg, cancellationToken);
                if (userCard != null)
                {
                    session.proof = RegistrationSession.IdentityProof.ATMCard;
                }
                else
                {
                    session.proof = RegistrationSession.IdentityProof.BVN;
                }
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(userCard != null ? "For security purposes, please provide the last 6 digits of your card." : "Please provide your BVN", UssdMessageType.ContinueSession); // ATM Card ??
            case 2:
                if (session.proof == RegistrationSession.IdentityProof.BVN)
                {
                    var bvnResult = await _registrationService.BvnValidation(requestDto.msg, cancellationToken);
                    if (bvnResult is null)
                    {
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("The BVN provided does not match our records.", UssdMessageType.EndSession);
                    }
                }
                else if (session.proof == RegistrationSession.IdentityProof.ATMCard)
                {
                    bool isLastSixDigitstrue = await _registrationService.ValidateLastSixDigits(requestDto.msg, session.accountNumber, cancellationToken);
                    if (!isLastSixDigitstrue)
                    {
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("That doesn't look quite right. Please try entering the last 6 digits of your card again.", UssdMessageType.EndSession);
                    }
                }
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("Please create your USSD PIN to secure your account.", UssdMessageType.ContinueSession);
            case 3:
                var validatePinResult = _registrationService.ValidatePinDataIntegrity(requestDto.msg);
                if (validatePinResult.errorResponseDto != null)
                {
                    return validatePinResult.errorResponseDto;
                }
                session.PIN = validatePinResult.pinStringHash;
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("Please re-enter your 4-digit PIN to confirm.", UssdMessageType.ContinueSession); ;
            case 4:
                var validatePinConfirmationResult = _registrationService.ValidatePinDataIntegrity(requestDto.msg);
                if (validatePinConfirmationResult.errorResponseDto != null)
                {
                    return validatePinConfirmationResult.errorResponseDto;
                }
                EntryResponseDto? errorMsg = await _registrationService.VerifyPinConfirmation(session, session.PIN!, requestDto.msg, cancellationToken);
                if (errorMsg != null) return errorMsg;

                await _registrationService.LinkInitialAccount(requestDto.msisdn, session.accountNumber, session.PIN!, cancellationToken);

                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("Your bank account has been successfully registered for USSD banking. You can add more accounts via Account Management.", UssdMessageType.EndSession);
            default: throw new ArgumentOutOfRangeException();
        }
    }

}

