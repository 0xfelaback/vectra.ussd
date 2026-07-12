using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class ServiceAccountConfiguration : IEntityTypeConfiguration<ServiceAccount>
{
    public void Configure(EntityTypeBuilder<ServiceAccount> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => new { b.IsPrimary, b.CustomerId });
        builder.Property(b => b.AccountNumber).HasMaxLength(10).IsFixedLength().IsRequired();
        builder.HasIndex(b => b.AccountNumber).IsUnique();
        builder.Property(b => b.PhoneNumber).HasMaxLength(15).IsRequired();
        builder.Property(b => b.BvnNumber).HasMaxLength(11).IsFixedLength().IsRequired();


        builder.HasOne(b => b.Customer).WithMany(c => c.ServiceAccounts).HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Cascade);


        builder.HasOne(b => b.CustomerAccount).WithOne(c => c.ServiceAccount);
    }
}