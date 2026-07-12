using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Vectra.Ussd.Application.Interfaces.Services;
using Vectra.Ussd.Domain.Entities.CoreBanking;

public sealed class TransferOrchestrator : ITransferOrchestrator
{
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly ICoreAccountValidationService _coreAccountValidationService;
    private readonly ITransferService _service;
    private readonly ILogger<TransferOrchestrator> _logger;
    private const int MAX_PIN_TRIALS = 5;
    private const string STERLING_BANK_CODE = "232";
    public TransferOrchestrator(ILogger<TransferOrchestrator> logger, ICoreAccountValidationService coreAccountValidationService, IHandleSession handlesession, IVerifySession verifySession, ITransferService service, IPasswordHasher<string> passwordHasher)
    {
        _handleSession = handlesession;
        _verifySession = verifySession;
        _service = service;
        _passwordHasher = passwordHasher;
        _coreAccountValidationService = coreAccountValidationService;
        _logger = logger;
    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        /* PERFORM REGISTRATION REDIRECT */
        var displayAccountsResult = await _service.DisplayServiceAccounts(requestDto.msisdn, cancellationToken);
        if (displayAccountsResult.userAccounts is null)
        {
            return displayAccountsResult.reponseDto;
        }

        if (!requestDto.msg.StartsWith("*822*"))
        {
            /* FROM MENU */
            TransferSession session = new TransferSession
            {
                SessionId = requestDto.sessionid,
                PhoneNumber = requestDto.msisdn,
                CurrentStep = 1,
                sub = SessionSub.Transfer,
                CreatedAt = DateTime.Now,
                fromMenu = true,
                userAccounts = displayAccountsResult.userAccounts
            };
            await _handleSession.SaveSessionAsync(session, cancellationToken);
            return displayAccountsResult.reponseDto;
        }
        else
        {
            /* FROM SHORT STRING */
            string[] result = requestDto.msg.Split("*", StringSplitOptions.RemoveEmptyEntries);
            if (result.Length != 3) return new EntryResponseDto("Invalid request format", UssdMessageType.EndSession);

            bool parsed = decimal.TryParse(result[1], out decimal transferAmount);
            if (!parsed) return new EntryResponseDto("Please try again by entering a valid nigerian currency amount.", UssdMessageType.ContinueSession);

            TransferSession session = new TransferSession
            {
                SessionId = requestDto.sessionid,
                PhoneNumber = requestDto.msisdn,
                CurrentStep = 3,
                sub = SessionSub.Transfer,
                transferAmount = transferAmount,
                CreatedAt = DateTime.Now,
                fromMenu = false,
                userAccounts = displayAccountsResult.userAccounts
            };

            AccountQueryDto? beneficiary = await _service.AccountNumberValidation(result[2], cancellationToken);
            if (beneficiary is null) return new EntryResponseDto("Beneficiary account does not exist.", UssdMessageType.ContinueSession);
            BvnQueryDto? beneficiarybBvnDetails = await _service.BvnValidation(beneficiary.bvnNumber, cancellationToken);
            if (beneficiary != null && beneficiary.status == CustomerAccount.AccountStatus.Dormant)
            {
                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("This account number is not permitted to receive incoming transfers", UssdMessageType.EndSession);
            }
            if (beneficiarybBvnDetails is null) return new EntryResponseDto("Core banking couldn't access beneficiary BVN records for identification.", UssdMessageType.ContinueSession);
            NubanQueryDto? nubanResult = await _service.GetIntrabankTransferBeneficiaryDetails(beneficiary!.accountNumber, cancellationToken);
            if (nubanResult is null || !nubanResult.IsActive)
            {
                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(nubanResult is null ? "This account number is not registered on any core banking records" : "The beneficiary account number is NOT active for use", UssdMessageType.EndSession);
            }
            session.beneficiaryBankCode = nubanResult.BankCode;
            session.beneficiaryFirstName = beneficiarybBvnDetails.FirstName;
            session.beneficiaryLastName = beneficiarybBvnDetails.LastName;
            session.beneficiaryAccountNumber = beneficiary!.accountNumber;
            if (displayAccountsResult.userAccounts is null)
            {
                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                return displayAccountsResult.reponseDto;
            }
            session.userAccounts = displayAccountsResult.userAccounts;
            await _handleSession.SaveSessionAsync(session, cancellationToken);
            return displayAccountsResult.reponseDto;
        }
    }

    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var result = await _verifySession.VerifySessionAsync<TransferSession>(sessionBase, cancellationToken);
        if (result.error != null)
        {
            return result.error;
        }
        TransferSession? session = result.session;

        switch (session!.CurrentStep)
        {
            case 1:
                bool parsed = decimal.TryParse(requestDto.msg, out decimal amount);
                if (!parsed) return new EntryResponseDto("Please try again by entering a valid nigerian currency amount.", UssdMessageType.ContinueSession);
                session.CurrentStep++;
                session.transferAmount = amount;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("Please enter the account number for the beneficiary.", UssdMessageType.ContinueSession);
            case 2:
                /* FROM MENU */
                AccountQueryDto? beneficiary = await _service.AccountNumberValidation(requestDto.msg, cancellationToken);
                if (beneficiary is null) return new EntryResponseDto("Beneficiary account does not exist.", UssdMessageType.ContinueSession);
                BvnQueryDto? beneficiaryBvnDetails = await _service.BvnValidation(beneficiary.bvnNumber, cancellationToken);
                if (beneficiaryBvnDetails is null) return new EntryResponseDto("Core banking couldn't access beneficiary BVN records for identification.", UssdMessageType.ContinueSession);
                NubanQueryDto? interbankBeneficiary = await _service.GetIntrabankTransferBeneficiaryDetails(beneficiary!.accountNumber, cancellationToken);
                if (interbankBeneficiary is null || !interbankBeneficiary.IsActive)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto(interbankBeneficiary is null ? "This account number is not registered on any core banking records" : "The beneficiary account number is NOT active for use", UssdMessageType.EndSession);
                }
                session.beneficiaryBankCode = interbankBeneficiary.BankCode;
                session.beneficiaryFirstName = beneficiaryBvnDetails.FirstName;
                session.beneficiaryLastName = beneficiaryBvnDetails.LastName;
                session.beneficiaryAccountNumber = beneficiary.accountNumber;
                /* FROM SHORT STRING HAD CONTINUED FROM HERE EALIER, I MOVED IT NOW TO 3 */

                var displayAccountsResult = await _service.DisplayServiceAccounts(requestDto.msisdn, cancellationToken);
                if (displayAccountsResult.userAccounts is null)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return displayAccountsResult.reponseDto;
                }
                session.CurrentStep++;
                session.userAccounts = displayAccountsResult.userAccounts;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return displayAccountsResult.reponseDto;
            case 3:
                bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
                if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                if (requestMsg < 1 || requestMsg > session.userAccounts!.Count) return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);

                var selectedAccount = session.userAccounts![requestMsg - 1];
                session.accountNumber = selectedAccount.AccountNumber;
                session.remitterAccountbalance = selectedAccount.CustomerAccount.Balance;
                session.accountTier = selectedAccount.CustomerAccount.TierLevel;
                session.remitterTierLevel = selectedAccount.CustomerAccount.TierLevel;
                session.ussdPin = selectedAccount.Customer.ussdPin1Hash;

                string? bankName = session.beneficiaryBankCode == STERLING_BANK_CODE
                    ? "STERLING BANK"
                    : await _service.GetBankNameByCode(session.beneficiaryBankCode!, cancellationToken);
                session.beneficiaryBankName = bankName!;
                if (session.beneficiaryBankCode == STERLING_BANK_CODE) session.isIntraBankTransfer = true;
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto($"TRANSFER ₦{session.transferAmount} FROM ACCOUNT {session.accountNumber} {bankName} to {session.beneficiaryFirstName} {session.beneficiaryLastName}. Input 4-digit PIN to confirm.", UssdMessageType.ContinueSession);
            case 4:
                if (session.CreatedAt < DateTime.Now.AddMinutes(-9.8)) return new EntryResponseDto("This session has expired. Please start a new transfer request.", UssdMessageType.EndSession);
                string transferIdempotencyKey = Guid.CreateVersion7().ToString("N");
                /*WRONG PIN CHECK*/
                PasswordVerificationResult res = _passwordHasher.VerifyHashedPassword(string.Empty, session.ussdPin ?? "", requestDto.msg);
                if (res == PasswordVerificationResult.Failed)
                {
                    session.pinTrials++;
                    await _service.IncrementAccountPinTrial(session.accountNumber, cancellationToken);
                    await _handleSession.SaveSessionAsync(session, cancellationToken);
                    if (session.pinTrials >= MAX_PIN_TRIALS)
                    {
                        try
                        {
                            await _service.InActivateServiceAccount(session.accountNumber, cancellationToken);
                            await _handleSession.RemoveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto("Too many incorrect PIN attempts. For security reasons, this account has been locked.", UssdMessageType.EndSession);
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError(exception, "Failed to deactivate account {AccountNumber} after max PIN trials.", session.accountNumber);

                        }
                    }
                    return new EntryResponseDto("The PIN you entered don't match. Please try again", UssdMessageType.ContinueSession);
                }
                /*INSUFFICIENT BALANCE CHECK*/
                bool isSufficientBalance = _service.BalanceCheck(session.transferAmount, session.remitterAccountbalance);
                if (!isSufficientBalance)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Insufficient balance. The transfer amount plus fees exceeds your available funds.", UssdMessageType.EndSession);
                }
                /* ACCOUNT CONSTRANT CHECK */
                string? tierConstrain = await _service.CheckAccountTierLimitExceeded(session.accountNumber, session.transferAmount, session.accountTier, session.isIntraBankTransfer ? TransferType.Intrabank : TransferType.Interbank, cancellationToken);
                if (tierConstrain != null)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto(tierConstrain, UssdMessageType.EndSession);
                }

                var accountValidationResult = await _coreAccountValidationService.AccountValidation(requestDto.msisdn, session.beneficiaryAccountNumber, cancellationToken);
                if (accountValidationResult.errorResonse != null) return accountValidationResult.errorResonse;

                /* EXECUTE TRANSFER - TODO: WRITE DUPLICATE CHECK FOR DUPLICATE TRANSFERS */
                string transactionref = await _service.ExecuteTransfer(session.accountNumber, session.beneficiaryAccountNumber,
                 session.transferAmount, session.isIntraBankTransfer ? TransferType.Intrabank : TransferType.Interbank,
                  TransactionHistory.TransactionStatus.Success, null, session.beneficiaryBankName, transferIdempotencyKey, cancellationToken);
                await _service.ResetAccountPinTrial(session.accountNumber, cancellationToken);
                /* RUN THE TRANSACTION - TODO: CHECK THAT ENDSESSION DISPAYS THE END MESSAGE */
                //await _handleSession.RemoveSessionAsync(session, cancellationToken);
                return new EntryResponseDto($"Transfer of ₦{session.transferAmount} to {session.beneficiaryFirstName} {session.beneficiaryLastName} was successful. Ref: {transactionref}", UssdMessageType.EndSession);

            default: throw new ArgumentOutOfRangeException();
        }
    }
}




