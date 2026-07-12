using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Mocks.CoreBanking;

public class AccountTierConfiguration : IEntityTypeConfiguration<AccountTier>
{
    public void Configure(EntityTypeBuilder<AccountTier> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TierLevel).IsRequired();
        builder.Property(x => x.TransferType).IsRequired();
        builder.Property(x => x.SingleTransactionLimit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.DailyTransactionLimit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.DailyTransactionCount).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasIndex(x => new { x.TierLevel, x.TransferType }).IsUnique();
    }
}
