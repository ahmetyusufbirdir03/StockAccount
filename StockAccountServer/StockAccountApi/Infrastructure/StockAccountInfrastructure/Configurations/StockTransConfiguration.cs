using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAccountDomain.Entities;

namespace StockAccountInfrastructure.Configurations;

public class StockTransConfiguration : IEntityTypeConfiguration<StockTrans>
{
    public void Configure(EntityTypeBuilder<StockTrans> builder)
    {
        builder.ToTable("StockTrans");

        builder.HasKey(st => st.Id);

        builder.Property(st => st.Type)
            .HasMaxLength(50);

        builder.Property(st => st.Quantity)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(st => st.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(st => st.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(st => st.Description)
            .HasMaxLength(500);

        builder.HasOne(st => st.Company)
            .WithMany(c => c.StockTransactions)
            .HasForeignKey(st => st.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.Stock)
            .WithMany(s => s.StockTransactions)
            .HasForeignKey(st => st.StockId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
