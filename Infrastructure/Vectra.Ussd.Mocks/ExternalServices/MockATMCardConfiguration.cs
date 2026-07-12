using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class MockATMCardConfiguration : IEntityTypeConfiguration<MockATMCard>
{
    public void Configure(EntityTypeBuilder<MockATMCard> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CardNumber).HasMaxLength(16).IsRequired();
        builder.Property(c => c.Type).IsRequired();
        builder.Property(c => c.Network).IsRequired();
        builder.Property(c => c.ExpirationDate).IsRequired();
        builder.Property(c => c.CVV).IsRequired();
        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.Contactless).IsRequired();
        builder.Property(c => c.InternationalUsage).IsRequired();
        builder.HasOne(c => c.CustomerAccount)
            .WithMany(a => a.Cards)
            .HasForeignKey(c => c.CustomerAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
