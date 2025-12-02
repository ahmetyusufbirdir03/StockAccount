using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Repositories;
using StockAccountDomain.Entities;
using StockAccountInfrastructure.Context;
using StockAccountInfrastructure.Repositories;

namespace StockAccountInfrastructure;

public static class Registration
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("LocalDbConnection")));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
        services.AddHttpContextAccessor();
    }
}
