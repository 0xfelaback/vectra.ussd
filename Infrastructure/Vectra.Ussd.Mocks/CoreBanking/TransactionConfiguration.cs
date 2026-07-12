using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Mocks.CoreBanking;

public class TransactionConfiguration : IEntityTypeConfiguration<TransactionHistory>
{
    public void Configure(EntityTypeBuilder<TransactionHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SenderAccountNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.SenderAccountNumber);
        builder.Property(x => x.ReceiverAccountNumber).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TransferType).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.DateCreated).IsRequired();
        builder.HasIndex(x => x.DateCreated);
        builder.Property(x => x.BankName).HasMaxLength(100);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(15);
        builder.HasIndex(x => x.IdempotencyKey);
        builder.HasIndex(x => x.TransactionId).IsUnique();
        builder.HasIndex(x => x.SenderAccountNumber);
        builder.HasIndex(x => x.ReceiverAccountNumber);
        builder.HasIndex(x => x.DateCreated);
    }
}
