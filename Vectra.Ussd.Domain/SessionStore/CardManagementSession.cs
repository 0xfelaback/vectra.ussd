public record CardManagementSession : SessionBase
{
    public CardManagementOperation? cardManagementOperation { get; set; } = null;
    public List<object> userCards = [];
    public object? cardForPINChange { get; set; } = null;
    public CardControlAction? cardControlAction = null;
    public string? ussdCardPinHash = null;

    public enum CardManagementOperation
    {
        CardActivation = 1, PINChange = 2, CardControl = 3

    }
    public enum CardControlAction
    {
        EnableAll = 1, DisableAll = 2, POSEnable = 3, POSDisable = 4, WebEnable = 5, WebDisable = 6, ATMEnable = 7, ATMDisable = 8
    }
}