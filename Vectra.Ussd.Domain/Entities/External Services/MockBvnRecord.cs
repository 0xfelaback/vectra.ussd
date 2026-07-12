public class MockBvnRecord
{
    public int Id { get; set; }
    public string BvnNumber { get; set; } = null!;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; } = null;
    public DateOnly DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? Address { get; set; } = null;
    public enum Gender
    {
        Male, Female
    }
    public Gender gender { get; set; }
    public string? SignatureUrl { get; set; } = null;
    public bool IsActive { get; set; } = false;
}