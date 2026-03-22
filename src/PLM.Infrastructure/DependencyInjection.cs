using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PLM.Application.Interfaces;
using PLM.Application.Services;
using PLM.Domain.Entities;
using PLM.Domain.Interfaces;
using PLM.Infrastructure.Data;
using PLM.Infrastructure.Repositories;

namespace PLM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────────
        services.AddDbContext<PlmDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(PlmDbContext).Assembly.FullName)));

        services.AddDbContextFactory<PlmDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(PlmDbContext).Assembly.FullName)),
            ServiceLifetime.Scoped);

        // ── Identity ─────────────────────────────────────────────
        // Identity registration moved to Web project Program.cs to access SignInManager

        // ── Repositories ─────────────────────────────────────────
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductVersionRepository, ProductVersionRepository>();
        services.AddScoped<IBoMRepository, BoMRepository>();
        services.AddScoped<IBoMVersionRepository, BoMVersionRepository>();
        services.AddScoped<IECORepository, ECORepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // ── Services ─────────────────────────────────────────────
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBoMService, BoMService>();
        services.AddScoped<IECOService, ECOService>();

        // ── Caching ──────────────────────────────────────────────
        services.AddMemoryCache();

        return services;
    }
}
