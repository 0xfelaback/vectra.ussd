using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Mocks;

public class MockSimSwapRecordConfiguration : IEntityTypeConfiguration<MockSimSwapRecord>
{
    public void Configure(EntityTypeBuilder<MockSimSwapRecord> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.Property(x => x.IsSwapped).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.LastSwapDate).IsRequired(false);
        builder.Property(x => x.IsServiceAvailable).IsRequired().HasDefaultValue(true);
        builder.ToTable("MockNibssSimSwapRecords");
    }
}