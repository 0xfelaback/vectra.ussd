public record CardQueryDto(int CustomerAccountId, MockATMCard.cardType Type, string CardNumber, MockATMCard.cardNetwork Network, bool IsActive, bool IsActivated, string? cardPINHash, bool PosEnabled, bool AtmEnabled, bool WebEnabled);

