public sealed class CardRequestOrchestrator : ICardRequestOrchestrator
{
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly ICardRequestService _cardRequestService;
    private readonly IBvnReadRepository _bvnReadRepository;
    private const decimal MastercardBalanceThreshold = 250000m;

    public CardRequestOrchestrator(IHandleSession handleSession, IBvnReadRepository bvnReadRepository, IVerifySession verifySession, ICardRequestService cardRequestService)
    {
        _handleSession = handleSession;
        _verifySession = verifySession;
        _bvnReadRepository = bvnReadRepository;
        _cardRequestService = cardRequestService;
    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        CardRequestSession session = new CardRequestSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.CardRequest
        };

        var displayAccountsResult = await _cardRequestService.DisplayServiceAccounts(requestDto.msisdn, cancellationToken);
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
        var verifyResult = await _verifySession.VerifySessionAsync<CardRequestSession>(sessionBase, cancellationToken);
        if (verifyResult.error != null)
        {
            return verifyResult.error;
        }
        CardRequestSession session = verifyResult.session!;

        switch (session.CurrentStep)
        {
            case 1:
                bool isAccParsed = int.TryParse(requestDto.msg, out int accSelection);
                if (!isAccParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                if (accSelection < 1 || accSelection > session.userAccounts!.Count) return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                ServiceAccount? selectedAccount = session.userAccounts![accSelection - 1];

                if (selectedAccount.CustomerAccount.Type == CustomerAccount.AccountType.Domiciliary)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Domiciliary accounts are not eligible for USSD card requests. Please visit your nearest branch for assistance.", UssdMessageType.EndSession);
                }
                session.accountNumber = selectedAccount.AccountNumber;
                session.accountTier = selectedAccount.CustomerAccount.TierLevel;
                session.accountType = selectedAccount.CustomerAccount.Type;
                session.bvn = selectedAccount.BvnNumber;

                var result = await _cardRequestService.GetAvailableCardsforCustomer(selectedAccount.CustomerAccount.TierLevel, selectedAccount.CustomerAccount.Type, MastercardBalanceThreshold, selectedAccount.CustomerAccount.Balance, selectedAccount.AccountNumber, cancellationToken);
                session.eligibleCardNetworks = result.eligiblecardnetworks;

                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                string availableCard = string.Join("%0A", result.eligiblecards);
                return new EntryResponseDto($"Please select your preferred card type:%0A%0A{availableCard}", UssdMessageType.ContinueSession);
            case 2:
                bool isCardSelected = int.TryParse(requestDto.msg, out int cardSelection);
                if (!isCardSelected) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                var validCards = (session.eligibleCardNetworks?.Split(',') ?? ["Verve"]).ToList();
                if (cardSelection < 1 || cardSelection > validCards.Count) return new EntryResponseDto("Please select a valid card option and try again.", UssdMessageType.ContinueSession);
                string selectedCardNetwork = validCards[cardSelection - 1].Trim();
                session.selectedCardNetwork = selectedCardNetwork;

                var bvnRecord = await _bvnReadRepository.QueryBvnByBVN(session.bvn!, cancellationToken);
                if (bvnRecord == null || string.IsNullOrEmpty(bvnRecord.Address))
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Unable to retrieve your address on file. Please contact customer care or visit a branch.", UssdMessageType.EndSession);
                }

                session.customerAddress = bvnRecord.Address;
                session.customerName = $"{bvnRecord.FirstName} {bvnRecord.LastName}";
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);

                return new EntryResponseDto($"Please confirm your address on file:%0A%0A{bvnRecord.Address}%0A%0AReply 1 to accept%0AReply 2 to decline", UssdMessageType.ContinueSession);

            case 3:
                bool isAddressConfirmationParsed = int.TryParse(requestDto.msg, out int addressConfirmation);
                if (!isAddressConfirmationParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                if (addressConfirmation == 2)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Please contact customer care or visit a branch to update your address before requesting a card.", UssdMessageType.EndSession);
                }
                if (addressConfirmation != 1)
                {
                    return new EntryResponseDto("Please enter 1 to accept or 2 to decline.", UssdMessageType.ContinueSession);
                }
                session.CurrentStep++;

                decimal deliveryFee = 0;
                if (session.accountType == CustomerAccount.AccountType.Savings)
                {
                    deliveryFee = 500;
                }
                else if (session.accountType == CustomerAccount.AccountType.Current)
                {
                    if (session.selectedCardNetwork?.Equals("Verve", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        deliveryFee = 0;
                    }
                    else if (session.selectedCardNetwork?.Equals("MasterCard", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        deliveryFee = 500;
                    }
                }

                await _handleSession.SaveSessionAsync(session, cancellationToken);
                string feeMessage = deliveryFee.ToString();
                return new EntryResponseDto(deliveryFee > 0 ? $"A delivery fee of {feeMessage} applies for this card request" : "The cost of this card is free" + ".%0A%0AReply 1 to proceed%0AReply 2 to cancel", UssdMessageType.ContinueSession);

            case 4:
                bool isFeeAcceptedParsed = int.TryParse(requestDto.msg, out int feeResponse);
                if (!isFeeAcceptedParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                if (feeResponse == 2)
                {
                    await _handleSession.RemoveSessionAsync(session, cancellationToken);
                    return new EntryResponseDto("Card request cancelled.", UssdMessageType.EndSession);
                }
                if (feeResponse != 1)
                {
                    return new EntryResponseDto("Please enter 1 to proceed or 2 to cancel.", UssdMessageType.ContinueSession);
                }
                session.CurrentStep++;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("Please enter your 4-digit USSD PIN to authorize this card request.", UssdMessageType.ContinueSession);

            case 5:
                var validateOldPinForPin2 = _cardRequestService.ValidatePinDataIntegrity(requestDto.msg);
                if (validateOldPinForPin2.errorResponseDto != null)
                {
                    return validateOldPinForPin2.errorResponseDto;
                }
                bool isVerifiedTrue = await _cardRequestService.VerifyPin1(requestDto.msg, requestDto.msisdn, cancellationToken);
                if (!isVerifiedTrue) return new EntryResponseDto("This password is wrong. Please try again.", UssdMessageType.EndSession);


                /* SEND & FORGET - Task.Run() TO AN EXTERNAL NOTIFICATION SERVICE */
                /*string notificationMsg = $"Dear {session.customerName}, your card is on its way and will be with you within 2 to 7 working days.";
                await notificationService.SendSmsAsync(requestDto.msisdn, notificationMsg);
                if (!string.IsNullOrEmpty(session.customerEmail)) await _notificationService.SendEmailAsync(session.customerEmail, "Card Request Notification", notificationMsg);*/
                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                return new EntryResponseDto("Your card is on its way and will be with you within 2 to 5 working days.", UssdMessageType.EndSession);

            default: throw new ArgumentOutOfRangeException();
        }
        //return new EntryResponseDto(string.Empty, UssdMessageType.ContinueSession);
    }
}

public interface ICardRequestOrchestrator
{
    Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken);
    Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken);
}