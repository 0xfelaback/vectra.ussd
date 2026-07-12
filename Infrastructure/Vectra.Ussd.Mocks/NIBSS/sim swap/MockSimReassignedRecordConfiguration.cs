using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Mocks;

public class MockSimReassignedRecordConfiguration : IEntityTypeConfiguration<MockSimReassignedRecord>
{
    public void Configure(EntityTypeBuilder<MockSimReassignedRecord> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.Property(x => x.IsReassigned).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.LastAssignedDate).IsRequired(false);
        builder.Property(x => x.IsServiceAvailable).IsRequired().HasDefaultValue(true);
        builder.ToTable("MockNibssSimReassignedRecords");
    }
}
