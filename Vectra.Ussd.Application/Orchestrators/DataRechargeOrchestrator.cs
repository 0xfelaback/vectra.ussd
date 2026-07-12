using System.Security.Cryptography.X509Certificates;

public class DataRechargeOrchestrator : IDataRechargeOrchestrator
{
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly IDataRechargeService _dataRechargeService;
    private const string _unrecognisedMsg = "Unrecognised request. Please try again by selecting a valid option";
    public DataRechargeOrchestrator(IHandleSession handlesession, IVerifySession verifySession,
     IDataRechargeService dataRechargeService)
    {
        _handleSession = handlesession;
        _verifySession = verifySession;
        _dataRechargeService = dataRechargeService;
    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var displayAccountsResult = await _dataRechargeService.DisplayServiceAccounts(requestDto.msisdn, cancellationToken);
        if (displayAccountsResult.userAccounts is null)
        {
            return displayAccountsResult.reponseDto;
        }

        DataRechargeSession<DataBundleQueryDto> session = new DataRechargeSession<DataBundleQueryDto>
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.DataPurchase,
            userAccounts = displayAccountsResult.userAccounts
        };
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return displayAccountsResult.reponseDto;
    }
    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto,
     CancellationToken cancellationToken)
    {
        var verifyResult = await _verifySession.VerifySessionAsync<DataRechargeSession<DataBundleQueryDto>>(sessionBase, cancellationToken);
        if (verifyResult.error != null)
        {
            return verifyResult.error;
        }
        DataRechargeSession<DataBundleQueryDto> session = verifyResult.session!;

        switch (session.CurrentStep)
        {
            case 1:
                bool isAccParsed = int.TryParse(requestDto.msg, out int accSelection);
                if (!isAccParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                if (accSelection < 1 || accSelection > session.userAccounts!.Count) return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);

                var selectedAccount = session.userAccounts![accSelection - 1];
                session.accountNumber = selectedAccount.AccountNumber;
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("Please select a network from the options;%0A%0A1.MTN%0A2.Airtel%0A3.Globacom%0A4.9mobile", UssdMessageType.ContinueSession);
            case 2:
                bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
                if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                AirtimeRecharge.AirtimeNetwork? userInputTelco = requestMsg switch
                {
                    1 => AirtimeRecharge.AirtimeNetwork.MTN,
                    2 => AirtimeRecharge.AirtimeNetwork.Airtel,
                    3 => AirtimeRecharge.AirtimeNetwork.Globacom,
                    4 => AirtimeRecharge.AirtimeNetwork.NineMobile,
                    _ => null
                };
                if (userInputTelco is null) return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
                session.CurrentStep++;
                session.beneficiaryISP = userInputTelco;
                IEnumerable<DataBundleQueryDto>? bundleResults = (await _dataRechargeService.GetActiveBundlesforTelco(userInputTelco.GetValueOrDefault(), cancellationToken))!.ToList();
                if (bundleResults is null) return new EntryResponseDto("There is no active data plans on records for this provider", UssdMessageType.ContinueSession);
                string displayPlans = string.Join("%0A", bundleResults.Select((item, index) => $"{index + 1}. {item.bundleName} - {item.price}. Valid for {item.validityDays} day(s)."));
                session.userDataBundleResult = bundleResults;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(displayPlans, UssdMessageType.ContinueSession);
            case 3:
                bool isInputParsed = int.TryParse(requestDto.msg, out int requestmsg);
                if (!isInputParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                DataBundleQueryDto? dataPlan = session.userDataBundleResult!.FirstOrDefault(x => x.Id == requestmsg);
                if (dataPlan is null) return new EntryResponseDto("Input isn't a valid option, please try again", UssdMessageType.ContinueSession);
                session.userSelectedDataPlan = dataPlan;
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("Please enter your current PIN1", UssdMessageType.ContinueSession);
            case 4:
                var validatePin = _dataRechargeService.ValidatePinDataIntegrity(requestDto.msg);
                if (validatePin.errorResponseDto != null)
                {
                    return validatePin.errorResponseDto;
                }
                bool? isVerifiedTrue = await _dataRechargeService.VerifyPin1(requestDto.msg, requestDto.msisdn, cancellationToken);
                if (isVerifiedTrue == null)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken); return new EntryResponseDto("This customer does not exist", UssdMessageType.EndSession);
                }
                if (!isVerifiedTrue.Value)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("This password is wrong. Please try again.", UssdMessageType.EndSession);
                }
                /* GET USER ACCOUNT FOR DEBIT AND DATA PURCHASE */
                string? transactionRef = await _dataRechargeService.DataPurchase(requestDto.msisdn, requestDto.msisdn, session.accountNumber,
                 session.userSelectedDataPlan!.price, session.beneficiaryISP!.Value, AirtimeRecharge.TransactionStatus.Pending, true, cancellationToken);
                if (string.IsNullOrEmpty(transactionRef))
                {
                    return new EntryResponseDto("The operation failed.%0APlease ensure account is funded then try again later.", UssdMessageType.EndSession);
                }
                return new EntryResponseDto($"Successful: You have successfully purchased {session.userSelectedDataPlan.bundleName} data for {requestDto.msg}. Your account has been debited. Ref: {transactionRef}", UssdMessageType.EndSession);
            default: throw new ArgumentOutOfRangeException();
        }
    }
}

