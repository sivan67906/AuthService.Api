using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<CommandDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("CommandDb")));
        services.AddDbContext<QueryDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("QueryDb")));

        services.AddScoped<ICommandDbContext>(sp => sp.GetRequiredService<CommandDbContext>());
        services.AddScoped<IQueryDbContext>(sp => sp.GetRequiredService<QueryDbContext>());

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<CommandDbContext>()
            .AddDefaultTokenProviders();

        // CRITICAL: Memory Cache for UserAuthorizationService performance optimization
        services.AddMemoryCache();

        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();

        return services;
    }
}
