using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAccountDomain.Entities;

namespace StockAccountInfrastructure.Configurations
{
    public class AccountCompanyConfiguration : IEntityTypeConfiguration<AccountCompany>
    {
        public void Configure(EntityTypeBuilder<AccountCompany> builder)
        {
            builder.ToTable("AccountCompany");

            builder.HasKey(ac => new { ac.CompanyId, ac.AccountId });

            // RELATIONS
            builder
                .HasOne(ac => ac.Account)
                .WithMany(a => a.AccountCompanies)
                .HasForeignKey(ac => ac.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(ac => ac.Company)
                .WithMany(c => c.AccountCompanies)
                .HasForeignKey(ac => ac.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // PROPERTIES
            builder.Property(ac => ac.Balance)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(ac => ac.LinkedAt)
                   .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
