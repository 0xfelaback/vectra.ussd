using AutoMapper.Internal.Mappers;

public sealed class CardManagementOrchestrator : ICardManagementOrchestrator
{
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private readonly ICardManagementService _cardManagementService;
    private const string _unrecognisedMsg = "Unrecognised request. Please try again by selecting a valid option";
    public CardManagementOrchestrator(ICardManagementService cardManagementService, IHandleSession handlesession, IVerifySession verifySession)
    {
        _handleSession = handlesession;
        _verifySession = verifySession;
        _cardManagementService = cardManagementService;
    }
    public async Task<EntryResponseDto> InitialRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        CardManagementSession session = new CardManagementSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            CurrentStep = 1,
            sub = SessionSub.CardManagement
        };
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        return new EntryResponseDto("1.Card Activation.%0A2.PIN Change.%0A3.Card Control", UssdMessageType.ContinueSession);
    }
    public async Task<EntryResponseDto> ContinuationRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        var verifyResult = await _verifySession.VerifySessionAsync<CardManagementSession>(sessionBase, cancellationToken);
        if (verifyResult.error != null)
        {
            return verifyResult.error;
        }
        CardManagementSession session = verifyResult.session!;

        if (session.cardManagementOperation == null)
        {
            switch (session.CurrentStep)
            {
                case 1:
                    bool isOpParsed = int.TryParse(requestDto.msg, out int opSelection);
                    if (!isOpParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                    switch (opSelection)
                    {
                        case 1:
                            session.cardManagementOperation = CardManagementSession.CardManagementOperation.CardActivation;
                            List<CardQueryDto>? userInActiveCards = await _cardManagementService.GetCardsAvailableforActivation(requestDto.msisdn, cancellationToken);
                            if (userInActiveCards is null)
                            {
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("No available cards to activate", UssdMessageType.EndSession);
                            }
                            session.userCards = userInActiveCards.Cast<object>().ToList();
                            await _handleSession.SaveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto($"Select a card to activate%0A {string.Join("%0A", userInActiveCards.Select((item, index) => $"{index + 1}. {item.Type} XXXX-XXXX-XXXX-{item.CardNumber[^4..]}"))}", UssdMessageType.ContinueSession);
                        case 2:
                            session.cardManagementOperation = CardManagementSession.CardManagementOperation.PINChange;
                            List<CardQueryDto>? userCards = await _cardManagementService.GetCardsAvailableforPinChange(requestDto.msisdn, cancellationToken);
                            if (userCards is null)
                            {
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("No available cards to modify", UssdMessageType.EndSession);
                            }
                            session.userCards = userCards.Cast<object>().ToList();
                            await _handleSession.SaveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto($"Select which card PIN you want to change.%0A {string.Join("%0A", userCards.Select((item, index) => $"{index + 1}. {item.Type} XXXX-XXXX-XXXX-{item.CardNumber[^4..]}"))}", UssdMessageType.ContinueSession);
                        case 3:
                            session.cardManagementOperation = CardManagementSession.CardManagementOperation.CardControl;
                            await _handleSession.SaveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto("Select an available Card control option%0A1. Enable All%0A2.Disable All - POS%0A3. ATM%0A4. WEB%0A5. Onboard card%0A6. Geoblocking", UssdMessageType.ContinueSession);
                        default: return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
                    }

                default: throw new ArgumentOutOfRangeException();
            }
        }
        switch (session.cardManagementOperation)
        {
            case CardManagementSession.CardManagementOperation.CardActivation:
                switch (session.CurrentStep)
                {
                    case 1:
                        bool isIntParsed = int.TryParse(requestDto.msg, out int requestmsg);
                        if (!isIntParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                        CardQueryDto selectedCard = (CardQueryDto)session.userCards[requestmsg - 1];
                        if (selectedCard is null)
                        {
                            return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                        }
                        await _cardManagementService.ActivateSelectedCard(selectedCard.CardNumber, cancellationToken);
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Your card has been successfully activated.", UssdMessageType.EndSession);
                    default: throw new ArgumentOutOfRangeException();
                }
            case CardManagementSession.CardManagementOperation.PINChange:
                switch (session.CurrentStep)
                {
                    case 1:
                        bool isintParsed = int.TryParse(requestDto.msg, out int requestInputMsg);
                        if (!isintParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                        CardQueryDto selectedCard = (CardQueryDto)session.userCards[requestInputMsg - 1];
                        if (selectedCard is null)
                        {
                            return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                        }
                        session.cardForPINChange = selectedCard;
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Please enter your current Card PIN", UssdMessageType.ContinueSession);
                    case 2:
                        var validateOldCardPin = _cardManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validateOldCardPin.errorResponseDto != null)
                        {
                            return validateOldCardPin.errorResponseDto;
                        }
                        bool isVerifiedTrue = await _cardManagementService.VerifyCardPin(requestDto.msg, requestDto.msisdn, cancellationToken);
                        if (!isVerifiedTrue) return new EntryResponseDto("This password is wrong. Please try again.", UssdMessageType.EndSession);
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Enter your new Card PIN", UssdMessageType.ContinueSession);
                    case 3:
                        var validateNewCardPin = _cardManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validateNewCardPin.errorResponseDto != null)
                        {
                            return validateNewCardPin.errorResponseDto;
                        }

                        session.ussdCardPinHash = validateNewCardPin.pinStringHash;
                        session.CurrentStep++;
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Re-enter your new Card PIN to confirm", UssdMessageType.ContinueSession);
                    case 4:
                        var validateConfirmPin = _cardManagementService.ValidatePinDataIntegrity(requestDto.msg);
                        if (validateConfirmPin.errorResponseDto != null)
                        {
                            return validateConfirmPin.errorResponseDto;
                        }

                        EntryResponseDto? errorMsg = _cardManagementService.VerifyPinConfirmation(session.ussdCardPinHash!, requestDto.msg);
                        if (errorMsg != null) return errorMsg;

                        await _cardManagementService.EditCardPIN(((CardQueryDto)session.cardForPINChange!).CardNumber, session.ussdCardPinHash!, cancellationToken);
                        await _handleSession.RemoveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto("Your new card PIN has been successfully reset.", UssdMessageType.EndSession);
                    default: throw new ArgumentOutOfRangeException();
                }
            case CardManagementSession.CardManagementOperation.CardControl:
                switch (session.CurrentStep)
                {
                    case 1:
                        bool isParsed = int.TryParse(requestDto.msg, out int requestMsg);
                        if (!isParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                        List<CardQueryDto>? userInActiveCards = await _cardManagementService.GetCardsAvailableforActivation(requestDto.msisdn, cancellationToken);
                        if (userInActiveCards is null)
                        {
                            await _handleSession.RemoveSessionAsync(session, cancellationToken);
                            return new EntryResponseDto("No available cards to modify", UssdMessageType.EndSession);
                        }
                        session.cardControlAction = requestMsg switch
                        {
                            1 => CardManagementSession.CardControlAction.EnableAll,
                            2 => CardManagementSession.CardControlAction.DisableAll,
                            3 => CardManagementSession.CardControlAction.POSEnable,
                            4 => CardManagementSession.CardControlAction.POSDisable,
                            5 => CardManagementSession.CardControlAction.WebEnable,
                            6 => CardManagementSession.CardControlAction.WebDisable,
                            7 => CardManagementSession.CardControlAction.ATMEnable,
                            8 => CardManagementSession.CardControlAction.ATMDisable,
                            _ => null
                        };
                        session.CurrentStep++;
                        session.userCards = userInActiveCards.Cast<object>().ToList();
                        await _handleSession.SaveSessionAsync(session, cancellationToken);
                        return new EntryResponseDto($"Select a card to modify%0A {string.Join("%0A", userInActiveCards.Select((item, index) => $"{index + 1}. {item.Type} XXXX-XXXX-XXXX-{item.CardNumber[^4..]}"))}", UssdMessageType.ContinueSession);
                    case 2:
                        switch (session.cardControlAction)
                        {
                            case CardManagementSession.CardControlAction.EnableAll:
                                bool isIntParsed = int.TryParse(requestDto.msg, out int requestmsg);
                                if (!isIntParsed) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                                CardQueryDto selectedCard = (CardQueryDto)session.userCards[requestmsg - 1];
                                if (selectedCard is null)
                                {
                                    return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                                }
                                await _cardManagementService.EnableAll(selectedCard.CardNumber, cancellationToken);
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Selected card has been successfully enabled for all services.", UssdMessageType.EndSession);
                            case CardManagementSession.CardControlAction.DisableAll:
                                if (!int.TryParse(requestDto.msg, out int requestmessage)) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                                CardQueryDto a = (CardQueryDto)session.userCards[requestmessage - 1];
                                if (a is null)
                                {
                                    return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                                }
                                await _cardManagementService.DisableAll(a.CardNumber, cancellationToken);
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Selected card has been successfully disabled for all services.", UssdMessageType.EndSession);
                            case CardManagementSession.CardControlAction.POSEnable:
                                if (!int.TryParse(requestDto.msg, out int x)) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                                CardQueryDto b = (CardQueryDto)session.userCards[x - 1];
                                if (b is null)
                                {
                                    return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                                }
                                await _cardManagementService.EnablePOSForCard(b.CardNumber, cancellationToken);
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Selected card has been successfully enabled for POS services.", UssdMessageType.EndSession);
                            case CardManagementSession.CardControlAction.POSDisable:
                                if (!int.TryParse(requestDto.msg, out int y)) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                                CardQueryDto c = (CardQueryDto)session.userCards[y - 1];
                                if (c is null)
                                {
                                    return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                                }
                                await _cardManagementService.DisablePOSForCard(c.CardNumber, cancellationToken);
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Selected card has been successfully disabled for POS services.", UssdMessageType.EndSession);
                            case CardManagementSession.CardControlAction.WebEnable:
                                if (!int.TryParse(requestDto.msg, out int z)) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                                CardQueryDto d = (CardQueryDto)session.userCards[z - 1];
                                if (d is null)
                                {
                                    return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                                }
                                await _cardManagementService.EnableWebForCard(d.CardNumber, cancellationToken);
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Selected card has been successfully enabled for web services.", UssdMessageType.EndSession);
                            case CardManagementSession.CardControlAction.WebDisable:
                                if (!int.TryParse(requestDto.msg, out int p)) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                                CardQueryDto e = (CardQueryDto)session.userCards[p - 1];
                                if (e is null)
                                {
                                    return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                                }
                                await _cardManagementService.DisableWebForCard(e.CardNumber, cancellationToken);
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Selected card has been successfully disabled for web services.", UssdMessageType.EndSession);
                            case CardManagementSession.CardControlAction.ATMEnable:
                                if (!int.TryParse(requestDto.msg, out int q)) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                                CardQueryDto f = (CardQueryDto)session.userCards[q - 1];
                                if (f is null)
                                {
                                    return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                                }
                                await _cardManagementService.EnableATMForCard(f.CardNumber, cancellationToken);
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Selected card has been successfully enabled for ATM services.", UssdMessageType.EndSession);
                            case CardManagementSession.CardControlAction.ATMDisable:
                                if (!int.TryParse(requestDto.msg, out int r)) return new EntryResponseDto("Input isn't a valid number, please try again", UssdMessageType.ContinueSession);
                                CardQueryDto g = (CardQueryDto)session.userCards[r - 1];
                                if (g is null)
                                {
                                    return new EntryResponseDto("Please select a valid option and try again.", UssdMessageType.ContinueSession);
                                }
                                await _cardManagementService.DisableATMForCard(g.CardNumber, cancellationToken);
                                await _handleSession.RemoveSessionAsync(session, cancellationToken);
                                return new EntryResponseDto("Selected card has been successfully disabled for ATM services.", UssdMessageType.EndSession);
                            default: throw new ArgumentOutOfRangeException();

                        }
                    default: throw new ArgumentOutOfRangeException();
                }
            default: throw new ArgumentOutOfRangeException();
        }
    }
}

