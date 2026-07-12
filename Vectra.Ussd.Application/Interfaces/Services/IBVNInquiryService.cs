using Microsoft.AspNetCore.Identity;

public interface IBVNInquiryService
{
    Task<EntryResponseDto?> VerifyUserPINWithTrials<T>(T session, int maxPinTrials, string phoneNumber, string enteredPin,
     CancellationToken cancellationToken) where T : BVNInquirySession;
    Task<PasswordVerificationResult> ValidatePinAsync(string phoneNumber, string enteredPin,
    CancellationToken cancellationToken);
    Task<(EntryResponseDto? errorMsg, string? userAccountNumber)> DebitCustomer<T>(T session, string phoneNumber,
     decimal queryAmount, CancellationToken cancellationToken) where T : BVNInquirySession;
}