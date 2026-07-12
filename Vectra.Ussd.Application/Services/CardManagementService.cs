using AutoMapper;
using Microsoft.AspNetCore.Identity;

public sealed class CardManagementService : ICardManagementService
{
    private readonly IMockATMCardRepository _mockATMCardRepository;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly IMapper _mapper;
    public CardManagementService(IMockATMCardRepository mockATMCardRepository, IPasswordHasher<string> passwordHasher, IMapper mapper)
    {
        _mockATMCardRepository = mockATMCardRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }
    public async Task<List<CardQueryDto>?> GetCardsAvailableforActivation(string customerPhoneNumber, CancellationToken token)
    {
        IEnumerable<MockATMCard> userCards = await _mockATMCardRepository.GetByCustomerPhoneNumberAsync(customerPhoneNumber, token);
        if (userCards is null) return null;
        return _mapper.Map<IEnumerable<CardQueryDto>>(userCards).Where(a => !a.IsActivated || a.IsActive).ToList();
    }
    public async Task<List<CardQueryDto>?> GetCardsAvailableforPinChange(string customerPhoneNumber, CancellationToken token)
    {
        IEnumerable<MockATMCard> userCards = await _mockATMCardRepository.GetByCustomerPhoneNumberAsync(customerPhoneNumber, token);
        if (userCards is null) return null;
        return _mapper.Map<IEnumerable<CardQueryDto>>(userCards).Where(a => a.cardPINHash != null).ToList();
    }
    public async Task ActivateSelectedCard(string cardNumber, CancellationToken cancellationToken)
    {
        await _mockATMCardRepository.ActivateATMCard(cardNumber, cancellationToken);
    }
    public (EntryResponseDto? errorResponseDto, string? pinStringHash) ValidatePinDataIntegrity(string pinStrng)
    {
        if (pinStrng.Length != 4) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        bool input = int.TryParse(pinStrng, out int value);
        if (!input) return (new EntryResponseDto("Please provide a valid 4-digit PIN", UssdMessageType.ContinueSession), null);
        string pinHash = _passwordHasher.HashPassword(string.Empty, pinStrng);
        return (null, pinHash);
    }
    public async Task<bool> VerifyCardPin(string providedPinHash, string cardNumber, CancellationToken cancellationToken)
    {
        MockATMCard? card = await _mockATMCardRepository.GetByCardNumberAsync(cardNumber, cancellationToken);
        PasswordVerificationResult varResult = _passwordHasher.VerifyHashedPassword(string.Empty, card!.cardPINHash ?? "", providedPinHash);
        if (varResult == PasswordVerificationResult.Failed) return false;
        return true;
    }
    public EntryResponseDto? VerifyPinConfirmation(string? cardPinHash, string providedPassword)
    {
        if (string.IsNullOrEmpty(cardPinHash)) return new EntryResponseDto("PIN couldn't be processed, the process failed.%0ATry again later", UssdMessageType.EndSession);
        PasswordVerificationResult res = _passwordHasher.VerifyHashedPassword(string.Empty, cardPinHash, providedPassword);
        if (res == PasswordVerificationResult.Failed) return new EntryResponseDto("The PIN you entered don't match. Please try again", UssdMessageType.ContinueSession);

        return null;
    }
    public async Task EditCardPIN(string cardNumber, string newPinHash, CancellationToken token) =>
        await _mockATMCardRepository.ChangePINAsync(cardNumber, newPinHash, token);

    public async Task EnableWebForCard(string cardNumber, CancellationToken cancellationToken) => await _mockATMCardRepository.EnableWEB(cardNumber, cancellationToken);
    public async Task EnablePOSForCard(string cardNumber, CancellationToken cancellationToken) => await _mockATMCardRepository.EnablePOS(cardNumber, cancellationToken);

    public async Task EnableATMForCard(string cardNumber, CancellationToken cancellationToken) => await _mockATMCardRepository.EnableATM(cardNumber, cancellationToken);

    public async Task DisableWebForCard(string cardNumber, CancellationToken cancellationToken) => await _mockATMCardRepository.DisableWEB(cardNumber, cancellationToken);
    public async Task DisablePOSForCard(string cardNumber, CancellationToken cancellationToken) => await _mockATMCardRepository.DisablePOS(cardNumber, cancellationToken);

    public async Task DisableATMForCard(string cardNumber, CancellationToken cancellationToken) => await _mockATMCardRepository.DisableATM(cardNumber, cancellationToken);


    public async Task EnableAll(string cardNumber, CancellationToken cancellationToken)
    {
        await EnableWebForCard(cardNumber, cancellationToken);
        await EnablePOSForCard(cardNumber, cancellationToken);
        await EnableATMForCard(cardNumber, cancellationToken);
    }
    public async Task DisableAll(string cardNumber, CancellationToken cancellationToken)
    {
        await DisableATMForCard(cardNumber, cancellationToken);
        await DisablePOSForCard(cardNumber, cancellationToken);
        await DisableWebForCard(cardNumber, cancellationToken);
    }

}

