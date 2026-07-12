public record AccountOpeningSession : RegistrationAndAccountOpeningSessionBase
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateOnly? DOB { get; set; } = null;
    public bool? BvnValidated { get; set; } = false;
    public bool isBvnActive { get; set; } = false;
    public string ImageUrl { get; set; } = null!;
    public string? SignatureUrl { get; set; }
}


