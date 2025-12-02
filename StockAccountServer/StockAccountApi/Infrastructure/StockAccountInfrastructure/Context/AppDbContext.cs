using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockAccountDomain.Entities;
using System.Reflection;

namespace StockAccountInfrastructure.Context;

public class AppDbContext : IdentityDbContext<User, Role, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<StockTrans> StockTransactions { get; set; }
    public DbSet<ActTrans> ActTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
