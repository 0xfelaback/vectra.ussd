using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Vectra.Ussd.Application.Interfaces.Services;

[ApiController]
[Route("api")]
public class EntryController : ControllerBase
{
    private readonly IVerifySession _verifySession;
    private readonly IAccountOpeningOrchestrator _accountOpeningOrchestrator;
    private readonly IMenuOrchestrator _menuOrchestrator;
    private readonly IRegistrationOrchestrator _registrationOrchestrator;
    private readonly ITransferOrchestrator _transferOrchestrator;
    private readonly IAccountBalanceOrchestrator _accountBalanceOrchestrator;
    private readonly IServiceAccountNumbersOrchestrator _ServiceAccountNumbersOrchestrator;
    private readonly IPINManagementOrchestrator _pinManagementOrchestrator;
    private readonly IAirtimeOrchestrator _airtimeOrchestrator;
    private readonly IBVNInquiryOrchestrator _bvnInquiryOrchestrator;
    private readonly IDataRechargeOrchestrator _dataRechargeOrchestrator;
    private readonly IAccountManagementOrchestrator _accountManagementOrchestrator;
    private readonly ICardRequestOrchestrator _cardRequestOrchestrator;
    private readonly ICardManagementOrchestrator _cardManagementOrchestrator;
    private readonly IDistributedCache _cache;
    private readonly ITransferService _transferService;

    public EntryController(IServiceAccountNumbersOrchestrator ServiceAccountNumbersOrchestrator, ITransferService transferService,
    IAccountBalanceOrchestrator accountBalanceOrchestrator,
     IAccountOpeningOrchestrator accountOpeningOrchestrator, IDistributedCache cache, IDataRechargeOrchestrator dataRechargeOrchestrator,
      IMenuOrchestrator menuOrchestrator, IRegistrationOrchestrator registrationOrchestrator, ICardRequestOrchestrator cardRequestOrchestrator, ICardManagementOrchestrator cardManagementOrchestrator,
      IPINManagementOrchestrator pinManagementOrchestrator, IAirtimeOrchestrator airtimeOrchestrator,
      IBVNInquiryOrchestrator bvnInquiryOrchestrator, IAccountManagementOrchestrator accountManagementOrchestrator,
       IVerifySession verifySession, ITransferOrchestrator transferOrchestrator)
    {
        _transferService = transferService;
        _accountOpeningOrchestrator = accountOpeningOrchestrator;
        _accountBalanceOrchestrator = accountBalanceOrchestrator;
        _ServiceAccountNumbersOrchestrator = ServiceAccountNumbersOrchestrator;
        _menuOrchestrator = menuOrchestrator;
        _bvnInquiryOrchestrator = bvnInquiryOrchestrator;
        _registrationOrchestrator = registrationOrchestrator;
        _pinManagementOrchestrator = pinManagementOrchestrator;
        _cardRequestOrchestrator = cardRequestOrchestrator;
        _airtimeOrchestrator = airtimeOrchestrator;
        _accountManagementOrchestrator = accountManagementOrchestrator;
        _cache = cache;
        _verifySession = verifySession;
        _dataRechargeOrchestrator = dataRechargeOrchestrator;
        _transferOrchestrator = transferOrchestrator;
        _cardManagementOrchestrator = cardManagementOrchestrator;
    }
    [HttpPost("ussd-channel")]
    [Consumes("application/xml")]
    public async Task<IActionResult> UssdEndpoint([FromBody] EntryRequestDto request)
    {
        EntryResponseDto? response = null;
        (EntryResponseDto? response, SessionBase? sessionBase) result;

        switch (request.type)
        {
            case UssdMessageType.InitialRequest:
                /* SHORT CODES + MENU */
                /* TODO: WHY NOT RE-DISPLAY THE RESPONSE OF THE LAST ACTION ON WRONG USER INPUT INSTEAD OF TERMINATION. OF THE 
                SESSION */
                /* TODO: SEND AIRTIME INFO TO AGGREGATOR USING BACKGROUND SERVICES */
                /* TODO: UPDATE FOR AVAILABLE ORCH */

                if (request.msg == "*822#")
                {
                    response = await _menuOrchestrator.InitialPageRequest(request, HttpContext.RequestAborted);
                }
                else if (request.msg == "*822*1#")
                {
                    response = await _registrationOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                }
                else if (request.msg == "*822*7#")
                {
                    response = await _accountOpeningOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                }
                else if (request.msg == "*822*6#")
                {
                    await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                    response = await _accountBalanceOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                }
                else if (request.msg == "*822*8#")
                {
                    await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                    response = await _ServiceAccountNumbersOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                }
                /* TODO: RESOLVE THIS STRUCTURED CONDITION BUG */
                else if (request.msg.StartsWith("*822*")) //822*amount*beneficiaryAccountNumber
                {
                    await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                    response = await _transferOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                }
                else if (request.msg.StartsWith("*822*")) //822*amount#
                {
                    await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                    response = await _airtimeOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                }
                else
                {
                    response = new EntryResponseDto("Unrecognised initial request", UssdMessageType.EndSession);
                }
                break;
            case UssdMessageType.ContinueSession:
                //assumed user already has session Id stored
                result = await HandleSessionAsyncRepeated(request.sessionid);
                switch (result.sessionBase!.sub)
                {
                    case SessionSub.mainMenu:
                        if ((request.msg == "99" || request.msg == "0") && (result.sessionBase.CurrentStep == 1 || result.sessionBase.CurrentStep == 2))
                        {
                            response = await _menuOrchestrator.NextPageRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "0" && result.sessionBase.CurrentStep == 0)
                        {
                            response = await _menuOrchestrator.InitialPageRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "1")
                        {
                            response = await _accountOpeningOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "2")
                        {
                            response = await _registrationOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "3")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _transferOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "5")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _airtimeOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "7")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _accountBalanceOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "8")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _ServiceAccountNumbersOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "10")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _dataRechargeOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "11")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _bvnInquiryOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "13")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _pinManagementOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "14")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _cardRequestOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "14")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _cardManagementOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        else if (request.msg == "18")
                        {
                            await RedirectUserToRegistrationNotUssdRegistered(request.msisdn, request, HttpContext.RequestAborted);
                            response = await _accountManagementOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
                        }
                        break;
                    case SessionSub.accountOpening:
                        result.response = await _accountOpeningOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.registration:
                        result.response = await _registrationOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.Transfer:
                        result.response = await _transferOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.accountBalance:
                        result.response = await _accountBalanceOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.checkAccountNumber:
                        result.response = await _ServiceAccountNumbersOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.PINManagement:
                        result.response = await _pinManagementOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.Airtime:
                        result.response = await _airtimeOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.BvnCheck:
                        result.response = await _bvnInquiryOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.DataPurchase:
                        result.response = await _dataRechargeOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.accountManagement:
                        result.response = await _accountManagementOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.CardRequest:
                        result.response = await _cardRequestOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    case SessionSub.CardManagement:
                        result.response = await _cardManagementOrchestrator.ContinuationRequest(result.sessionBase, request, HttpContext.RequestAborted);
                        response = result.response;
                        break;
                    default: break;
                }
                break;
            case UssdMessageType.EndSession:
                result = await HandleSessionAsyncRepeated(request.sessionid);
                if (result.sessionBase is null) return Unauthorized(result.response);
                await _cache.RemoveAsync(result.sessionBase.SessionId, HttpContext.RequestAborted);
                result.response = new EntryResponseDto("Thank you for using our service.%0AHave a wonderful day.", UssdMessageType.ContinueSession);
                response = result.response;
                break;
            default: throw new ArgumentOutOfRangeException();
        }
        return Ok(response);
    }
    private async Task<(EntryResponseDto? response, SessionBase? sessionBase)> HandleSessionAsyncRepeated(string sessionId)
    {
        /* REPEATED LOGIC - TODO: FIX AND DELETE */
        EntryResponseDto? response = null;
        string sessionJson = await _cache.GetStringAsync(sessionId, HttpContext.RequestAborted) ?? string.Empty;
        if (string.IsNullOrEmpty(sessionJson)) response = new EntryResponseDto("This session does not exist.", UssdMessageType.EndSession);
        SessionBase? sessionBase = JsonSerializer.Deserialize<SessionBase>(sessionJson) ?? null;
        if (sessionBase is null) response = new EntryResponseDto("This session does not exist.", UssdMessageType.EndSession);
        return (response, sessionBase);
    }

    public async Task<IActionResult?> RedirectUserToRegistrationNotUssdRegistered(string phoneNumber, EntryRequestDto request, CancellationToken cancellationToken)
    {
        bool isUserUssdRegistered = false;
        AccountQueryDto? registeredUser = await _transferService.ServiceAccountValidation(phoneNumber, cancellationToken);
        if (registeredUser is null) return Ok(new EntryResponseDto("There is no existing bank account linked to this phone number.%0APlease dial *822*7# to open an account*", UssdMessageType.EndSession));
        if (registeredUser.isUssdRegistered) isUserUssdRegistered = true;
        if (!isUserUssdRegistered)
        {
            EntryResponseDto response = await _registrationOrchestrator.InitialRequest(request, HttpContext.RequestAborted);
            return Ok(response);
        }
        return null;
    }
}


