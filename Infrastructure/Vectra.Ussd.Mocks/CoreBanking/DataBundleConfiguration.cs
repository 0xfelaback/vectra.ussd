using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Mocks.CoreBanking;

public class DataBundleConfiguration : IEntityTypeConfiguration<DataBundle>
{
    public void Configure(EntityTypeBuilder<DataBundle> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Telco).IsRequired();
        builder.Property(x => x.BundleName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DataSizeMB).IsRequired();
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ValidityDays).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasIndex(x => new { x.Telco, x.BundleName }).IsUnique();
    }
}
