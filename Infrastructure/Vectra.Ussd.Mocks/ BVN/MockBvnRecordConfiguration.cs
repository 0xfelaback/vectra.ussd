using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Mocks;

public class MockBvnRecordConfiguration : IEntityTypeConfiguration<MockBvnRecord>
{
    public void Configure(EntityTypeBuilder<MockBvnRecord> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BvnNumber).HasMaxLength(11).IsFixedLength().IsRequired();
        builder.Property(b => b.FirstName).IsRequired();
        builder.Property(b => b.LastName).IsRequired();
        builder.Property(b => b.DateOfBirth).IsRequired();
        builder.Property(b => b.PhoneNumber).IsRequired();
        builder.Property(b => b.ImageUrl).IsRequired();
        builder.Property(b => b.gender).IsRequired();
        builder.Property(b => b.IsActive).IsRequired();
        builder.ToTable("MockBvnRecords");
    }
}