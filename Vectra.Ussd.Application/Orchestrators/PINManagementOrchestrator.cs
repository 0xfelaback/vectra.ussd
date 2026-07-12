using Microsoft.AspNetCore.Identity;

public sealed class PINManagementOrchestrator : IPINManagementOrchestrator
{
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly IPINManagementService _pinManagementService;
    private readonly IPasswordHasher<string> _passwordHasher;
    private const string _unrecognisedMsg = "Unrecognised request. Please try again by selecting a valid option";
    public PINManagementOrchestrator(IHandleSession handlesession, IVerifySession verifySession, IPINManagementService pinManagementService,
      IPasswordHasher<string> passwordHasher)
    {
        _handleSession = handlesession;
        _verifySession = verifySession;
        _passwordHasher = passwordHasher;
        _pinManagementService = pinManagementService;
    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        PINManagementSession session = new PINManagementSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.PINManagement
        };
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return new EntryResponseDto("1.PIN Creation.%0A2.PIN Reset.", UssdMessageType.ContinueSession);
    }




    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var verifyResult = await _verifySession.VerifySessionAsync<PINManagementSession>(sessionBase, cancellationToken);
        if (verifyResult.error != null)
        {
            return verifyResult.error;
        }
        PINManagementSession session = verifyResult.session!;


        switch (session.CurrentStep)
        {

            case 1:
                bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
                if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                session.CurrentStep++;
                if (requestMsg == 1)
                {
                    /* CREATE PIN */
                    session.pinOperation = PINManagementSession.PINOperations.CreatePIN;
                    await _handleSession.SaveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Please provide your account number", UssdMessageType.ContinueSession);
                }
                else if (requestMsg == 2)
                {
                    /* RESET PIN */
                    session.pinOperation = PINManagementSession.PINOperations.ResetPIN;
                    await _handleSession.SaveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Please provide your account number", UssdMessageType.ContinueSession);
                }
                else
                {
                    return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
                }
            case 2:
                IEnumerable<AccountQueryDto>? ServiceAccounts = await _pinManagementService.GetServiceAccounts(requestDto.msisdn, cancellationToken);
                if (ServiceAccounts is null || !ServiceAccounts.Any())
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("The account number is not linked to this phone number", UssdMessageType.EndSession);
                }
                List<AccountQueryDto> accounts = ServiceAccounts.ToList();
                AccountQueryDto? seslectedAccount = accounts.FirstOrDefault(a => a.accountNumber == requestDto.msg);
                if (seslectedAccount is null)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("The selected account was not found.", UssdMessageType.EndSession);
                }

                CardQueryDto? userCard = await _pinManagementService.GetOneCardFromAccount(seslectedAccount.accountNumber, cancellationToken);
                session.ussdPin1Hash = seslectedAccount.ussdPin1Hash;
                session.ussdPin2Hash = seslectedAccount.ussdPin2Hash;
                session.accountNumber = seslectedAccount.accountNumber;
                var bvnDetails = await _pinManagementService.BvnValidationByPhoneNumber(requestDto.msisdn, cancellationToken);
                if (bvnDetails != null) session.DOB = bvnDetails.DateOfBirth;
                if (userCard != null)
                {
                    session.proof = PINManagementSession.IdentityProof.ATMCard;
                }
                else
                {
                    session.proof = PINManagementSession.IdentityProof.BVN;
                }
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(userCard != null ? "For security purposes, please provide the last 6 digits of your ATM card." :
                 "Please provide your BVN", UssdMessageType.ContinueSession);
            case 3:
                if (session.proof == PINManagementSession.IdentityProof.BVN)
                {
                    var bvnResult = await _pinManagementService.BvnValidation(requestDto.msg, cancellationToken);
                    if (bvnResult is null)
                    {
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("The BVN provided does not match our records.", UssdMessageType.EndSession);
                    }
                }
                else if (session.proof == PINManagementSession.IdentityProof.ATMCard)
                {
                    bool islastSixDigitstrue = await _pinManagementService.ValidateLastSixDigits(requestDto.msg, session.accountNumber, cancellationToken);
                    if (!islastSixDigitstrue)
                    {
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("The entered input is false. Please restart the process again.",
                         UssdMessageType.EndSession);
                    }
                }
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(session.pinOperation == PINManagementSession.PINOperations.CreatePIN ? "1.Create PIN 1%0A2.Create PIN 2" :
                 "1.Reset PIN %0A2.Reset PIN 2", UssdMessageType.ContinueSession);
            case 4:

                switch (requestDto.msg, session.pinOperation)
                {
                    case ("1", PINManagementSession.PINOperations.CreatePIN):
                        /* CREATE PIN 1 */
                        session.pinActions = PINManagementSession.PINAction.createPIN1;

                        if (session.ussdPin1Hash != null)
                        {
                            await _handleSession.RemoveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto("A PIN1 is already set.%0APlease use the 'Reset PIN1' option to change it.",
                         UssdMessageType.EndSession);
                        }
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please enter your PIN1 below", UssdMessageType.ContinueSession);
                    case ("2", PINManagementSession.PINOperations.CreatePIN):
                        /* CREATE PIN 2 */
                        session.pinActions = PINManagementSession.PINAction.createPIN2;
                        if (session.ussdPin2Hash != null)
                        {
                            await _handleSession.RemoveSessionAsync(session, cancellationToken); return new EntryResponseDto("A PIN2 is already set.%0APlease use the 'Reset PIN2' option to change it.",
                         UssdMessageType.EndSession);
                        }
                        session.CurrentStep = 8;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please enter your current PIN1", UssdMessageType.ContinueSession);
                    case ("1", PINManagementSession.PINOperations.ResetPIN):
                        /* RESET PIN 1 */
                        session.pinActions = PINManagementSession.PINAction.resetPIN1;
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please enter your current PIN1", UssdMessageType.ContinueSession);
                    case ("2", PINManagementSession.PINOperations.ResetPIN):
                        /* RESET PIN 2 */
                        session.pinActions = PINManagementSession.PINAction.resetPIN2;
                        session.CurrentStep = 8;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please enter your current PIN1", UssdMessageType.ContinueSession);
                    default: throw new ArgumentOutOfRangeException();
                }
            case 5:
                switch (session.pinActions)
                {
                    case PINManagementSession.PINAction.createPIN1:
                        var validatePinResult = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validatePinResult.errorResponseDto != null)
                        {
                            return validatePinResult.errorResponseDto;
                        }
                        session.ussdPin1Hash = validatePinResult.pinStringHash;
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please re-enter your 4-digit PIN to confirm.", UssdMessageType.ContinueSession);
                    case PINManagementSession.PINAction.createPIN2:
                        var validatePin2Result = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validatePin2Result.errorResponseDto != null)
                        {
                            return validatePin2Result.errorResponseDto;
                        }
                        session.ussdPin2Hash = validatePin2Result.pinStringHash;
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please re-enter your 4-digit PIN to confirm.", UssdMessageType.ContinueSession);
                    case PINManagementSession.PINAction.resetPIN1:
                        var validateOldPin = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validateOldPin.errorResponseDto != null)
                        {
                            return validateOldPin.errorResponseDto;
                        }
                        bool isVerifiedPin1True = await _pinManagementService.VerifyPin1(requestDto.msg, requestDto.msisdn, cancellationToken);
                        if (!isVerifiedPin1True) return new EntryResponseDto("This password is wrong. Please try again.", UssdMessageType.EndSession);
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Enter your new 4-digit PIN1", UssdMessageType.ContinueSession);
                    case PINManagementSession.PINAction.resetPIN2:
                        var validateNewPin2 = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validateNewPin2.errorResponseDto != null)
                        {
                            return validateNewPin2.errorResponseDto;
                        }
                        session.ussdPin2Hash = validateNewPin2.pinStringHash;
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Re-enter your new PIN2 to confirm", UssdMessageType.ContinueSession);
                    default: throw new ArgumentOutOfRangeException();
                }
            case 6:
                switch (session.pinActions)
                {
                    case PINManagementSession.PINAction.createPIN1:
                        var validatePinConfirmationResult = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validatePinConfirmationResult.errorResponseDto != null)
                        {
                            return validatePinConfirmationResult.errorResponseDto;
                        }
                        EntryResponseDto? errorMsg = _pinManagementService.VerifyPinConfirmation(session.ussdPin1Hash!, requestDto.msg);
                        if (errorMsg != null) return errorMsg;
                        await _pinManagementService.CreateUSSDPin1(session.accountNumber, session.ussdPin1Hash!, cancellationToken);
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Your PIN 1 has been successfully created..",
                         UssdMessageType.EndSession);
                    case PINManagementSession.PINAction.createPIN2:
                        var validatePin2ConfirmationResult = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validatePin2ConfirmationResult.errorResponseDto != null)
                        {
                            return validatePin2ConfirmationResult.errorResponseDto;
                        }
                        errorMsg = _pinManagementService.VerifyPinConfirmation(session.ussdPin2Hash!, requestDto.msg);
                        if (errorMsg != null) return errorMsg;
                        await _pinManagementService.CreateUSSDPin2(session.accountNumber, session.ussdPin2Hash!, cancellationToken);
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Your PIN 2 has been successfully created..", UssdMessageType.EndSession);
                    case PINManagementSession.PINAction.resetPIN1:
                        var validateNewPin = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validateNewPin.errorResponseDto != null)
                        {
                            return validateNewPin.errorResponseDto;
                        }

                        session.ussdPin1Hash = validateNewPin.pinStringHash;
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Re-enter your new PIN1 to confirm", UssdMessageType.ContinueSession);
                    case PINManagementSession.PINAction.resetPIN2:
                        var validateConfirmPin2 = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validateConfirmPin2.errorResponseDto != null)
                        {
                            return validateConfirmPin2.errorResponseDto;
                        }
                        errorMsg = _pinManagementService.VerifyPinConfirmation(session.ussdPin2Hash!, requestDto.msg);
                        if (errorMsg != null) return errorMsg;

                        await _pinManagementService.EditUSSDPin2(session.accountNumber, session.ussdPin2Hash!, cancellationToken);
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Your PIN2 has been successfully reset.", UssdMessageType.EndSession);
                    default: throw new ArgumentOutOfRangeException();
                }
            case 7:
                /* THIS STEP SOLELY IS USED ONLY BE RESET PIN 1 */
                switch (session.pinActions)
                {
                    case PINManagementSession.PINAction.resetPIN1:
                        var validateConfirmPin = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validateConfirmPin.errorResponseDto != null)
                        {
                            return validateConfirmPin.errorResponseDto;
                        }

                        EntryResponseDto? errorMsg = _pinManagementService.VerifyPinConfirmation(session.ussdPin1Hash!, requestDto.msg);
                        if (errorMsg != null) return errorMsg;

                        await _pinManagementService.EditUSSDPin1(session.accountNumber, session.ussdPin1Hash!, cancellationToken);
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Your PIN1 has been successfully reset.", UssdMessageType.EndSession);
                    default: throw new ArgumentOutOfRangeException();
                }
            /* STEPS 8-10 ARE SOLELY FOR THE PURPOSE OF IDENTITY VERIFICATION - FOR PIN 2 CREATION AND RESET */
            case 8:
                var validateOldPinForPin2 = _pinManagementService.ValidatePinDataIntegrity(requestDto.msg);
                if (validateOldPinForPin2.errorResponseDto != null)
                {
                    return validateOldPinForPin2.errorResponseDto;
                }
                bool isVerifiedTrue = await _pinManagementService.VerifyPin1(requestDto.msg, requestDto.msisdn, cancellationToken);
                if (!isVerifiedTrue) return new EntryResponseDto("This password is wrong. Please try again.", UssdMessageType.EndSession);
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("Please enter your Date of Birth (DDMMYYYY)", UssdMessageType.ContinueSession);
            case 9:
                if (!DateOnly.TryParseExact(requestDto.msg, "ddMMyyyy", out DateOnly dob))
                {
                    return new EntryResponseDto("Invalid date format. Please enter date of birth in DD-MM-YYYY format", UssdMessageType.ContinueSession);
                }
                if (session.DOB != dob)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Date of Birth does not match our records.", UssdMessageType.EndSession);
                }
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("For security purposes, please provide the last 6 digits of your ATM card.", UssdMessageType.ContinueSession);
            case 10:
                bool isLastSixDigitstrue = await _pinManagementService.ValidateLastSixDigits(requestDto.msg, session.accountNumber, cancellationToken);
                if (!isLastSixDigitstrue)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("The entered input is false. Please restart the process again.", UssdMessageType.EndSession);
                }
                session.CurrentStep = 5;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(session.pinActions == PINManagementSession.PINAction.resetPIN2 ?
                    "Please enter your new PIN2" :
                    "Please enter your PIN2 below", UssdMessageType.ContinueSession);
            default:
                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.EndSession);
        }
    }







}
