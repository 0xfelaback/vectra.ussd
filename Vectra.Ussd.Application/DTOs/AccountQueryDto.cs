public record AccountQueryDto(string accountNumber, string phoneNumber,
 string bvnNumber, decimal Balance, int TierLevel, bool isUssdRegistered, ICollection<MockATMCard> cards,
  CustomerAccount.AccountStatus status, string? ussdPin1Hash, string? ussdPin2Hash);


