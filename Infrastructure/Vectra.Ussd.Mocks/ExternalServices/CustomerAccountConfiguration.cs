using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CustomerAccountConfiguration : IEntityTypeConfiguration<CustomerAccount>
{
    public void Configure(EntityTypeBuilder<CustomerAccount> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(b => b.CustomerId);
        builder.Property(b => b.AccountNumber).HasMaxLength(10).IsFixedLength().IsRequired();
        builder.HasIndex(b => b.AccountNumber).IsUnique();
        builder.Property(b => b.PhoneNumber).HasMaxLength(15).IsRequired();
        builder.Property(b => b.BvnNumber).HasMaxLength(11).IsFixedLength().IsRequired();
        builder.Property(b => b.Type).IsRequired();
        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.HasPnd).IsRequired();
        builder.Property(b => b.DateCreated).IsRequired();
        builder.Property(b => b.Balance).IsRequired();
        builder.Property(b => b.IsUssdRegistered).IsRequired();

        builder.HasMany(b => b.Cards)
            .WithOne(c => c.CustomerAccount)
            .HasForeignKey(c => c.CustomerAccountId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasOne(b => b.Customer).WithMany(c => c.CustomerAccounts).HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Cascade);

    }
}