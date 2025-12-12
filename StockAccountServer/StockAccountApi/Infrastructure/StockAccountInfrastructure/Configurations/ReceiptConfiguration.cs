using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAccountDomain.Entities;

namespace StockAccountInfrastructure.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipt");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .HasMaxLength(100);

        builder.Property(r => r.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.HasOne(r => r.Company)
            .WithMany(c => c.Receipts)
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Account)
            .WithMany(c => c.Receipts)
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Stock)
            .WithMany(c => c.Receipts)
            .HasForeignKey(r => r.StockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.ActTransactions)
            .WithOne(c => c.Receipt)
            .HasForeignKey(r => r.ReceiptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

