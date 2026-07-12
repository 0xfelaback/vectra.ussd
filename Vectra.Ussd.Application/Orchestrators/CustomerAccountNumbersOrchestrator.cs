public sealed class ServiceAccountNumbersOrchestrator : IServiceAccountNumbersOrchestrator
{
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly IServiceAccountNumbersService _service;
    private const int MAX_PIN_TRIALS = 3;
    private const decimal QUERY_FEE = 6.59m;

    public ServiceAccountNumbersOrchestrator(
        IHandleSession handleSession,
        IVerifySession verifySession,
        IServiceAccountNumbersService service
        )
    {
        _handleSession = handleSession;
        _verifySession = verifySession;
        _service = service;
    }

    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        ServiceAccountNumbersSession session = new ServiceAccountNumbersSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.checkAccountNumber
        };

        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return new EntryResponseDto("Please enter your USSD PIN to view your account numbers.", UssdMessageType.ContinueSession);
    }
    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var verifyResult = await _verifySession.VerifySessionAsync<ServiceAccountNumbersSession>(sessionBase, cancellationToken);
        if (verifyResult.error != null)
        {
            return verifyResult.error;
        }
        ServiceAccountNumbersSession session = verifyResult.session!;

        EntryResponseDto? pinErrorMsg = await _service.VerifyUserPINWithTrials(session, MAX_PIN_TRIALS, requestDto.msisdn, requestDto.msg, cancellationToken);
        if (pinErrorMsg != null) return pinErrorMsg;

        var customerDebitResult = await _service.DebitCustomer(session, requestDto.msisdn, QUERY_FEE, cancellationToken);
        if (customerDebitResult.errorMsg != null) return customerDebitResult.errorMsg;

        var displayAccountsResult = await _service.DisplayServiceAccounts(requestDto.msisdn, cancellationToken);
        if (displayAccountsResult.userAccounts is null)
        {
            await _handleSession.RemoveSessionAsync(session, cancellationToken);
            return displayAccountsResult.reponseDto;
        }
        string accounts = string.Join("%0A", displayAccountsResult.userAccounts.Select((item, index) => $"{index + 1}. {item.AccountNumber}"));
        string responseMessage = "Your Account Number(s): " + $"%0A{accounts}" + $"%0A%0AA Query fee of ₦{QUERY_FEE} has been debited from your account.%0A%0Thank you for banking with us.";
        await _handleSession.RemoveSessionAsync(session, cancellationToken);
        return new EntryResponseDto(responseMessage, UssdMessageType.EndSession);
    }
}