public interface ICardManagementService
{
    Task<List<CardQueryDto>?> GetCardsAvailableforActivation(string customerPhoneNumber, CancellationToken token);
    Task<List<CardQueryDto>?> GetCardsAvailableforPinChange(string customerPhoneNumber, CancellationToken token);
    Task ActivateSelectedCard(string cardNumber, CancellationToken cancellationToken);
    (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng);
    Task<bool> VerifyCardPin(string providedPinHash, string cardNumber, CancellationToken cancellationToken);
    EntryResponseDto? VerifyPinConfirmation(string cardPinHash, string providedPassword);
    Task EditCardPIN(string cardNumber, string newPinHash, CancellationToken token);
    Task EnableWebForCard(string cardNumber, CancellationToken cancellationToken);
    Task EnablePOSForCard(string cardNumber, CancellationToken cancellationToken);
    Task EnableATMForCard(string cardNumber, CancellationToken cancellationToken);
    Task DisableWebForCard(string cardNumber, CancellationToken cancellationToken);
    Task DisablePOSForCard(string cardNumber, CancellationToken cancellationToken);
    Task DisableATMForCard(string cardNumber, CancellationToken cancellationToken);
    Task EnableAll(string cardNumber, CancellationToken cancellationToken);
    Task DisableAll(string cardNumber, CancellationToken cancellationToken);
}