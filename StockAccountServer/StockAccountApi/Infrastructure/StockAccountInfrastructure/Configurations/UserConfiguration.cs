using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAccountDomain.Entities;

namespace StockAccountInfrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(u => u.Surname)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(u => u.RefreshToken)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(u => u.RefreshTokenExpiryTime)
               .IsRequired(false);

        builder.Ignore(u => u.FullName);

        builder.HasMany(u => u.Companies)
               .WithOne(c => c.User)
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}