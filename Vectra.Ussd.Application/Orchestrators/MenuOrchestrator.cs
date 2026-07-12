using Microsoft.Extensions.Caching.Distributed;

public sealed class MenuOrchestrator : IMenuOrchestrator
{
    private readonly IDistributedCache _cache;
    private readonly List<string> _menuItems;
    private readonly IHandleSession _handleSession;
    private readonly IVerifySession _verifySession;
    private string _unrecognisedMsg;
    private string next;
    private string prev;
    public MenuOrchestrator(IDistributedCache cache, IHandleSession handlesession, IVerifySession verifySession)
    {
        _cache = cache;
        _handleSession = handlesession;
        _verifySession = verifySession;
        _menuItems = new List<string>
{
    "1.Open Account%0A",
    "2.Registration%0A",
    "3.Transfer to Sterling Account%0A",
    "4.Transfer to Other Bank Account%0A",
    "5.Airtime Purchase%0A",
    //"6.Airtime Others%0A",
    "7.Account Balance%0A",
    "8.My Account Numbers%0A",
    "9.Bills Payment%0A%0A",
    "10.Data Purchase%0A",
    "11.Check BVN%0A",
    "12.Bank Statement%0A",
    "13.PIN Management%0A",
    "14.Card Request%0A",
    "15.Card Management%0A",
    "16.Cardless Withdrawal%0A",
    "17.Hide/Show Balance%0A%0A",
    "18.Account Management%0A",
    "19.USSD on POS%0A",
    "20.Block Series%0A",
    "21.Micro-Lending%0A",
    "22.Third party integrations%0A",
};
        next = "99.Next%0A";
        prev = "0.Previous%0A";
        _unrecognisedMsg = "Unrecognised request. Please try again by selecting a valid option";
    }
    public async Task<EntryResponseDto> InitialPageRequest(EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        MenuSession session = new MenuSession
        {
            SessionId = requestDto.sessionid,
            PhoneNumber = requestDto.msisdn,
            sub = SessionSub.mainMenu,
            currentPage = 1,
        };
        await _handleSession.SaveSessionAsync(session, cancellationToken);
        string responseMsg = string.Join("", _menuItems.Skip(0).Take(8));
        string firstPageMsg = responseMsg + "%0A%0A" + next;
        return new EntryResponseDto(firstPageMsg, UssdMessageType.ContinueSession);
    }
    public async Task<EntryResponseDto> NextPageRequest(SessionBase sessionBase, EntryRequestDto requestDto, CancellationToken cancellationToken)
    {
        /* "99" - next, "0" - prev */
        var result = await _verifySession.VerifySessionAsync<MenuSession>(sessionBase, cancellationToken);
        if (result.error != null)
        {
            return result.error;
        }
        MenuSession? session = result.session;

        string firstPageMsg = string.Join("", _menuItems.Skip(0).Take(8)) + "%0A%0A" + next;
        string secondPageMsg = string.Join("", _menuItems.Skip(8).Take(8)) + "%0A%0A" + prev + "%0A%0A" + next;
        string finalPageMsg = string.Join("", _menuItems.Skip(16).Take(8)) + "%0A%0A" + prev;

        if (session!.currentPage == 1)
        {
            if (requestDto.msg == "99")
            {
                session.currentPage = 2;
                session.CurrentStep = 2;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(secondPageMsg, UssdMessageType.ContinueSession);
            }
            else if (requestDto.msg == "0")
            {
                return new EntryResponseDto("Already on first page.%0A%0A" + firstPageMsg, UssdMessageType.ContinueSession);
            }
            else
            {
                return new EntryResponseDto(_unrecognisedMsg + "%0A%0A" + firstPageMsg, UssdMessageType.ContinueSession);
            }
        }
        else if (session.currentPage == 2)
        {
            if (requestDto.msg == "99")
            {
                session.currentPage = 3;
                session.CurrentStep = 3;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(finalPageMsg, UssdMessageType.ContinueSession);
            }
            else if (requestDto.msg == "0")
            {
                session.currentPage = 1;
                session.CurrentStep = 1;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(firstPageMsg, UssdMessageType.ContinueSession);
            }
            else
            {
                return new EntryResponseDto(_unrecognisedMsg + "%0A%0A" + secondPageMsg, UssdMessageType.ContinueSession);
            }
        }
        else if (session.currentPage == 3)
        {
            if (requestDto.msg == "0")
            {
                session.currentPage = 2;
                session.CurrentStep = 2;
                await _handleSession.SaveSessionAsync(session, cancellationToken);
                return new EntryResponseDto(secondPageMsg, UssdMessageType.ContinueSession);
            }
            else
            {
                return new EntryResponseDto(_unrecognisedMsg + "%0A%0A" + finalPageMsg, UssdMessageType.ContinueSession);
            }
        }
        else
        {
            return new EntryResponseDto(_unrecognisedMsg, UssdMessageType.ContinueSession);
        }
    }

}

