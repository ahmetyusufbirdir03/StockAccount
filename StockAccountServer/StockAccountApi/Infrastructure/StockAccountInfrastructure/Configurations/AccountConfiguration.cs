using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAccountDomain.Entities;

namespace StockAccountInfrastructure.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountName)
               .HasMaxLength(200)
               .IsRequired(false);

        builder.Property(a => a.PhoneNumber)
               .HasMaxLength(50)
               .IsRequired(false);

        builder.Property(a => a.Email)
               .HasMaxLength(150)
               .IsRequired(false);

        builder.Property(at => at.Balance)
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);

        builder.Property(a => a.Address)
               .HasMaxLength(500)
               .IsRequired(false);

        // NAVIGATION PROPERTIES
        builder
            .HasOne(a => a.Company)
            .WithMany(c => c.Accounts)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Receipts)
            .WithOne(r => r.Account)
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ActTransactions)
            .WithOne(at => at.Account)
            .HasForeignKey(at => at.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
