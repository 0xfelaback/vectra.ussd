public sealed class AirtimeOrchestrator : IAirtimeOrchestrator
{
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly IAirtimeService _airtimeService;
    private const string _unrecognisedMsg = "Unrecognised request. Please try again by selecting a valid option";
    public AirtimeOrchestrator(IHandleSession handlesession, IVerifySession verifySession, IAirtimeService airtimeService)
    {
        _handleSession = handlesession;
        _verifySession = verifySession;
        _airtimeService = airtimeService;
    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var displayAccountsResult = await _airtimeService.DisplayServiceAccounts(requestDto.msisdn, cancellationToken);
        if (displayAccountsResult.userAccounts is null)
        {
            return displayAccountsResult.reponseDto;
        }

        AirtimeSession session = new AirtimeSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.Airtime,
            userAccounts = displayAccountsResult.userAccounts
        };
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return displayAccountsResult.reponseDto;
    }
    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var verifyResult = await _verifySession.VerifySessionAsync<AirtimeSession>(sessionBase, cancellationToken);
        if (verifyResult.error != null)
        {
            return verifyResult.error;
        }
        AirtimeSession session = verifyResult.session!;

        if (session.airtimeOperation == null)
        {
            switch (session.CurrentStep)
            {
                case 1:
                    bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
                    if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                    if (requestMsg < 1 || requestMsg > session.userAccounts!.Count) return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);

                    var selectedAccount = session.userAccounts![requestMsg - 1];
                    session.accountNumber = selectedAccount.AccountNumber;
                    session.CurrentStep++;
                    await _handleSession.SaveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("1.Airtime for Self.%0A2.Airtime for Others.", UssdMessageType.ContinueSession);
                case 2:
                    if (session.airtimeOperation == null)
                    {
                        bool isOpParsed = int.TryParse(requestDto.msg, out int opSelection);
                        if (!isOpParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                        switch (opSelection)
                        {
                            case 1:
                                session.airtimeOperation = AirtimeSession.AirtimeOperation.Self;
                                await _handleSession.SaveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Input the amount you'd like to purchase", UssdMessageType.ContinueSession);
                            case 2:
                                session.airtimeOperation = AirtimeSession.AirtimeOperation.Others;
                                await _handleSession.SaveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Input the amount you'd like to purchase", UssdMessageType.ContinueSession);
                            default: return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
                        }

                    }
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }


        switch (session.airtimeOperation)
        {
            case AirtimeSession.AirtimeOperation.Self:
                bool isAmountParsed = decimal.TryParse(requestDto.msg, out decimal transferAmount);
                if (!isAmountParsed) return new EntryResponseDto("Please try again by entering a valid nigerian currency amount.", UssdMessageType.ContinueSession);
                string? transactionRef = await _airtimeService.CreateAirtimeTransaction(requestDto.msisdn, requestDto.msisdn,
                 session.accountNumber, transferAmount, (AirtimeRecharge.AirtimeNetwork)requestDto.network,
                  AirtimeRecharge.TransactionStatus.Pending, true, cancellationToken);
                if (string.IsNullOrEmpty(transactionRef))
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("The operation failed, please try again later", UssdMessageType.EndSession);
                }
                return new EntryResponseDto($"Successful: You have successfully purchased ₦{transferAmount} airtime for your number {requestDto.msisdn}. Your account has been debited. Ref: {transactionRef}", UssdMessageType.EndSession);

            case AirtimeSession.AirtimeOperation.Others:
                switch (session.CurrentStep)
                {
                    case 2:
                        isAmountParsed = decimal.TryParse(requestDto.msg, out transferAmount);
                        if (!isAmountParsed) return new EntryResponseDto("Please try again by entering a valid nigerian currency amount.", UssdMessageType.ContinueSession);
                        session.airtimeAmount = transferAmount;
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please select beneficiary ISP network%0A%0A1.MTN%0A2.Airtel%0A3.Globacom%0A4.9mobile", UssdMessageType.ContinueSession);
                    case 3:
                        bool isparsed = int.TryParse(requestDto.msg, out int userSelection);
                        if (!isparsed) return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
                        AirtimeRecharge.AirtimeNetwork? network = userSelection switch
                        {
                            1 => AirtimeRecharge.AirtimeNetwork.MTN,
                            2 => AirtimeRecharge.AirtimeNetwork.Airtel,
                            3 => AirtimeRecharge.AirtimeNetwork.Globacom,
                            4 => AirtimeRecharge.AirtimeNetwork.NineMobile,
                            _ => null
                        };
                        if (network is null) return new EntryResponseDto("Please try again by entering a valid network from the options", UssdMessageType.ContinueSession);
                        session.beneficiaryISP = network;
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please enter your current PIN1", UssdMessageType.ContinueSession);
                    case 4:
                        var validateOldPin = _airtimeService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validateOldPin.errorResponseDto != null)
                        {
                            return validateOldPin.errorResponseDto;
                        }
                        bool? isVerifiedTrue = await _airtimeService.VerifyPin1(requestDto.msg, requestDto.msisdn, cancellationToken);
                        if (isVerifiedTrue == null)
                        {
                            await _handleSession.RemoveSessionAsync(session, cancellationToken); return new EntryResponseDto("This customer does not exist", UssdMessageType.EndSession);
                        }

                        if (!isVerifiedTrue.Value)
                        {
                            await _handleSession.RemoveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto("This password is wrong. Please try again.", UssdMessageType.EndSession);
                        }
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please enter the phone number of intended beneficiary below.", UssdMessageType.ContinueSession);
                    case 5:
                        var filteredResult = _airtimeService.ValidateBeneficiaryPhoneNumber(requestDto.msg);
                        if (filteredResult.errorResponse != null) return filteredResult.errorResponse;

                        transactionRef = await _airtimeService.CreateAirtimeTransaction(requestDto.msisdn, requestDto.msg,
                             session.accountNumber, session.airtimeAmount!.Value, session.beneficiaryISP!.Value,
                              AirtimeRecharge.TransactionStatus.Pending, false, cancellationToken);

                        if (string.IsNullOrEmpty(transactionRef))
                        {
                            return new EntryResponseDto("The operation failed.%0APlease ensure account is funded then try again later.", UssdMessageType.EndSession);
                        }
                        return new EntryResponseDto($"Successful: You have successfully purchased ₦{session.airtimeAmount.Value} airtime for {requestDto.msg}. Your account has been debited. Ref: {transactionRef}", UssdMessageType.EndSession);
                    default: return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
                }
            default: throw new ArgumentOutOfRangeException();
        }
    }


}

public interface IAirtimeOrchestrator
{
    Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken);
    Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken);
}