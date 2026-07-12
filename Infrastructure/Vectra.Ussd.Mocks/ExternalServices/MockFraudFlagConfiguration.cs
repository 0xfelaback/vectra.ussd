
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MockFraudFlagConfiguration : IEntityTypeConfiguration<MockFraudFlag>
{
    public void Configure(EntityTypeBuilder<MockFraudFlag> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.PhoneNumber).HasMaxLength(11).IsFixedLength().IsRequired();
        builder.Property(b => b.BvnNumber).HasMaxLength(15).IsFixedLength().IsRequired();
        builder.Property(b => b.Reason).IsRequired();
        builder.Property(b => b.Timestamp).IsRequired();
        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.AccountNumber).HasMaxLength(10).IsFixedLength();
    }
}