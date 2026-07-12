public sealed class AccountManagementOrchestrator : IAccountManagementOrchestrator
{
    private readonly IAccountManagementService _accountManagementService;
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private const string _unrecognisedMsg = "Unrecognised request. Please try again by selecting a valid option";
    public AccountManagementOrchestrator(IHandleSession handlesession, IVerifySession verifySession, IAccountManagementService accountManagementService)
    {
        _accountManagementService = accountManagementService;
        _handleSession = handlesession;
        _verifySession = verifySession;
    }

    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        AccountManagementSession session = new AccountManagementSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.accountManagement,
        };
        CustomrQueryDto? customerDetails = await _accountManagementService.GetCustomerDetails(requestDto.msisdn, cancellationToken);
        if (customerDetails == null)
        {
            return new EntryResponseDto("This customer does not exist", UssdMessageType.EndSession);
        }
        session.customerAccountId = customerDetails.customerId;
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return new EntryResponseDto("1. Set Primary Account.%0A2. Add Account.%0A3. Remove Account", UssdMessageType.ContinueSession);
    }
    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var verifyResult = await _verifySession.VerifySessionAsync<AccountManagementSession>(sessionBase, cancellationToken);
        if (verifyResult.error != null)
        {
            return verifyResult.error;
        }
        AccountManagementSession session = verifyResult.session!;

        if (session.accountManagementOperation is null)
        {
            bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
            if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
            switch (requestMsg)
            {
                case 1:
                    session.accountManagementOperation = AccountManagementSession.AccountManagementOperation.SetPrimaryAccount;
                    session.CurrentStep++;
                    await _handleSession.SaveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Enter preferred account number", UssdMessageType.ContinueSession);
                case 2:
                    session.accountManagementOperation = AccountManagementSession.AccountManagementOperation.AddAccount;
                    session.CurrentStep++;
                    await _handleSession.SaveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Enter account number to add", UssdMessageType.ContinueSession);
                case 3:
                    session.accountManagementOperation = AccountManagementSession.AccountManagementOperation.RemoveAccount;
                    session.CurrentStep++;
                    await _handleSession.SaveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Enter account number to remove", UssdMessageType.ContinueSession);
                default: return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
            }
        }

        switch (session.accountManagementOperation)
        {
            case AccountManagementSession.AccountManagementOperation.SetPrimaryAccount:
                switch (session.CurrentStep)
                {
                    case 3:
                        session.CurrentStep++; session.accountNumber = requestDto.msg;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto($"Confirm setting {requestDto.msg} as primary? 1. Yes 2. No", UssdMessageType.ContinueSession);
                    case 4:
                        bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
                        if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                        if (requestMsg == 1)
                        {
                            (EntryResponseDto? errorResponse, bool isSuccess) result = await _accountManagementService.SetPrimaryAccount(requestDto.msisdn, session.accountNumber, cancellationToken);
                            if (!result.isSuccess)
                            {
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return result.errorResponse!;
                            }
                            string responsemsg = $"Success! Account number ending in ****{session.accountNumber[^4..]} has been successfully set as your primary account. All future transactions will default to this account.";
                            await _handleSession.RemoveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto(responsemsg, UssdMessageType.EndSession);
                        }
                        else if (requestMsg == 2)
                        {
                            return new EntryResponseDto("Request aborted", UssdMessageType.EndSession);
                        }
                        else { return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession); }

                    default: throw new ArgumentOutOfRangeException();
                }

            case AccountManagementSession.AccountManagementOperation.AddAccount:
                switch (session.CurrentStep)
                {
                    case 3:
                        session.CurrentStep++; session.accountNumber = requestDto.msg;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto($"Confirm adding {requestDto.msg}? 1. Yes 2. No", UssdMessageType.ContinueSession);
                    case 4:
                        bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
                        if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                        if (requestMsg == 1)
                        {
                            (EntryResponseDto? errorResponse, bool isSuccess) result = await _accountManagementService.AddAccount(requestDto.msisdn, session.accountNumber, cancellationToken);
                            if (!result.isSuccess)
                            {
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return result.errorResponse!;
                            }
                            string responsemsg = $"Success! Account ending in ****{session.accountNumber[^4..]} has been successfully added to your accounts. You can now use it for transactions.";
                            await _handleSession.RemoveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto(responsemsg, UssdMessageType.EndSession);
                        }
                        else if (requestMsg == 2)
                        {
                            return new EntryResponseDto("Request aborted", UssdMessageType.EndSession);
                        }
                        else { return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession); }
                    default: throw new ArgumentOutOfRangeException();
                }
            case AccountManagementSession.AccountManagementOperation.RemoveAccount:
                switch (session.CurrentStep)
                {
                    case 3:
                        session.CurrentStep++; session.accountNumber = requestDto.msg;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto($"Confirm removing {requestDto.msg}? 1. Yes 2. No", UssdMessageType.ContinueSession);
                    case 4:
                        bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
                        if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                        if (requestMsg == 1)
                        {
                            (EntryResponseDto? errorResponse, bool isSuccess) result = await _accountManagementService.RemoveAccount(requestDto.msisdn, session.accountNumber, cancellationToken);
                            if (!result.isSuccess)
                            {
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return result.errorResponse!;
                            }
                            string responsemsg = $"Success! Account ending in ****{session.accountNumber[^4..]} has been removed from your accounts";
                            await _handleSession.RemoveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto(responsemsg, UssdMessageType.EndSession);
                        }
                        else if (requestMsg == 2)
                        {
                            return new EntryResponseDto("Request aborted", UssdMessageType.EndSession);
                        }
                        else { return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession); }
                    default: throw new ArgumentOutOfRangeException();
                }
            default: return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
        }
    }

}

