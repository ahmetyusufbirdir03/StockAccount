using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAccountDomain.Entities;

namespace StockAccountInfrastructure.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stock");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Quantity)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.Unit)
            .HasMaxLength(50);

        builder.Property(s => s.Price)
            .HasColumnType("decimal(18,2)") 
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.HasOne(s => s.Company)
            .WithMany(c => c.Stocks)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.StockTransactions)
            .WithOne(c => c.Stock)
            .HasForeignKey(s => s.StockId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
