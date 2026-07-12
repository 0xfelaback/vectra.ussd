
public sealed class AccountBalanceOrchestrator : IAccountBalanceOrchestrator
{
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    public AccountBalanceOrchestrator(IAccountBalanceService accountBalanceService, IHandleSession handlesession, IVerifySession verifySession)
    {
        _accountBalanceService = accountBalanceService;
        _handleSession = handlesession;
        _verifySession = verifySession;
    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        AccountBalanceSession session = new AccountBalanceSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.accountBalance,
        };

        var displayAccountsResult = await _accountBalanceService.DisplayServiceAccounts(requestDto.msisdn, cancellationToken);
        if (displayAccountsResult.userAccounts is null)
        {
            await _handleSession.RemoveSessionAsync(session, cancellationToken);
            return displayAccountsResult.reponseDto;
        }
        session.userAccounts = displayAccountsResult.userAccounts;
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return displayAccountsResult.reponseDto;
    }

    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var result = await _verifySession.VerifySessionAsync<AccountBalanceSession>(sessionBase, cancellationToken);
        if (result.error != null)
        {
            return result.error;
        }
        AccountBalanceSession? session = result.session;



        bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
        if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
        var selectedAccount = session!.userAccounts![requestMsg - 1];
        if (selectedAccount is null)
        {
            return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
        }

        decimal? accountBalance = await _accountBalanceService.GetAccountBalance(selectedAccount.AccountNumber, cancellationToken);
        if (accountBalance is null) return new EntryResponseDto("The operation failed. No account found linked to this phone number.", UssdMessageType.EndSession);
        return new EntryResponseDto($"Dear Customer, your current balance is ₦{accountBalance}. Thank you for banking with us.", UssdMessageType.EndSession);
    }


}

