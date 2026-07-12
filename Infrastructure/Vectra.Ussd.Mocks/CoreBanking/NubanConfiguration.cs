using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class NubanConfiguration : IEntityTypeConfiguration<NubanInterbankAccounts>
{
    public void Configure(EntityTypeBuilder<NubanInterbankAccounts> builder)
    {
        builder.HasKey(b => b.AccountNumber);
        builder.Property(b => b.AccountNumber).HasMaxLength(10).IsFixedLength().IsRequired();
        builder.Property(b => b.AccountName).HasMaxLength(100).IsRequired();
        builder.Property(b => b.BankCode).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(b => b.BankName).HasMaxLength(100).IsRequired();
        builder.Property(b => b.IsActive).IsRequired();
    }
}