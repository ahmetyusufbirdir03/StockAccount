using StockAccountDomain.Entities;

namespace StockAccountInfrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(15);

        builder.Property(x => x.Email)
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .HasMaxLength(255);

        // UNIQUE INDEXES
        builder.HasIndex(x => x.CompanyName).IsUnique();
        builder.HasIndex(x => x.PhoneNumber).IsUnique(true); 
        builder.HasIndex(x => x.Email).IsUnique(true); 

        // USER 1 → M COMPANY
        builder.HasOne(c => c.User)
            .WithMany(u => u.Companies) 
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // COMAPANY 1 -> M ACCOUNT
        builder.HasMany(c => c.Accounts)
            .WithOne(s => s.Company)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // COMPANY 1 → M STOCK
        builder.HasMany(c => c.Stocks)
            .WithOne(s => s.Company)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // COMPANY 1 → M STOCKTRANS
        builder.HasMany(c => c.StockTransactions)
            .WithOne(st => st.Company)
            .HasForeignKey(st => st.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // COMPANY 1 → M ACTTRANS
        builder.HasMany(c => c.ActTransactions)
            .WithOne(at => at.Company)
            .HasForeignKey(at => at.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // COMPANY 1 → M RECEIPT
        builder.HasMany(c => c.Receipts)
            .WithOne(r => r.Company)
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

