using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAccountDomain.Entities;

namespace StockAccountInfrastructure.Configurations;

public class ActTransConfiguration : IEntityTypeConfiguration<ActTrans>
{
    public void Configure(EntityTypeBuilder<ActTrans> builder)
    {
        builder.ToTable("ActTrans");

        builder.HasKey(at => at.Id);

        builder.Property(at => at.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(at => at.Description)
            .HasMaxLength(100);

        builder.HasOne(at => at.Company)
            .WithMany(c => c.ActTransactions)
            .HasForeignKey(at => at.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(at => at.Account)
            .WithMany(c => c.ActTransactions)
            .HasForeignKey(at => at.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(at => at.Receipt)
            .WithMany(r => r.ActTransactions)
            .HasForeignKey(at => at.ReceiptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
