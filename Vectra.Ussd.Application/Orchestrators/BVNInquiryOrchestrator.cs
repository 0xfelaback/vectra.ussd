public sealed class BVNInquiryOrchestrator : IBVNInquiryOrchestrator
{
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly IBVNInquiryService _bvnInquiryService;
    private const int MAX_PIN_TRIALS = 3;
    private const decimal QUERY_FEE = 6.59m;
    public BVNInquiryOrchestrator(IVerifySession verifySession, IHandleSession handleSession, IBVNInquiryService bvnInquiryService)
    {
        _handleSession = handleSession;
        _verifySession = verifySession;
        _bvnInquiryService = bvnInquiryService;
    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        BVNInquirySession session = new BVNInquirySession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.BvnCheck
        };

        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return new EntryResponseDto("Please enter your USSD PIN to view your BVN.", UssdMessageType.ContinueSession);
    }
    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var verifyResult = await _verifySession.VerifySessionAsync<BVNInquirySession>(sessionBase, cancellationToken);
        if (verifyResult.error != null)
        {
            return verifyResult.error;
        }
        BVNInquirySession session = verifyResult.session!;

        EntryResponseDto? errorMsg = await _bvnInquiryService.VerifyUserPINWithTrials(session, MAX_PIN_TRIALS, requestDto.msisdn, requestDto.msg, cancellationToken);
        if (errorMsg != null) return errorMsg;


        var customerDebitResult = await _bvnInquiryService.DebitCustomer(session, requestDto.msisdn, QUERY_FEE, cancellationToken);
        if (customerDebitResult.errorMsg != null) return customerDebitResult.errorMsg;

        string responseMessage = $"Your BVN Number:%0A{customerDebitResult.userAccountNumber}%0A%0AA Query fee of ₦{QUERY_FEE} has been debited from your account.%0A%0Thank you for banking with us.";
        await _handleSession.RemoveSessionAsync(session, cancellationToken);
        return new EntryResponseDto(responseMessage, UssdMessageType.EndSession);
    }
}

