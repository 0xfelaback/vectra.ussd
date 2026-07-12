namespace Vectra.Ussd.Domain.Entities.CoreBanking;

public class DataBundle
{
    public int Id { get; set; }
    public AirtimeRecharge.AirtimeNetwork Telco { get; set; }
    public string BundleName { get; set; } = null!;
    public int DataSizeMB { get; set; }
    public decimal Price { get; set; }
    public int ValidityDays { get; set; }
    public bool IsActive { get; set; } = true;
}
