using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public class CommandDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, System.Guid>, ICommandDbContext
{
    public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options)
    {
    }

    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserRefreshToken> RefreshTokens => Set<UserRefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("ApplicationUsers");
        });
        builder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("ApplicationRoles");
        });
        builder.Entity<IdentityUserRole<System.Guid>>(b =>
        {
            b.ToTable("UserRoles");
        });
        builder.Entity<IdentityUserLogin<System.Guid>>(b =>
        {
            b.ToTable("UserLogins");
        });
        builder.Entity<IdentityUserToken<System.Guid>>(b =>
        {
            b.ToTable("UserTokens");
        });
        builder.Entity<IdentityRoleClaim<System.Guid>>(b =>
        {
            b.ToTable("RoleClaims");
        });
        builder.Entity<IdentityUserClaim<System.Guid>>(b =>
        {
            b.ToTable("UserClaims");
        });

        builder.Entity<UserAddress>(b =>
        {
            b.ToTable("UserAddresses");
        });

        builder.Entity<UserRefreshToken>(b =>
        {
            b.ToTable("UserRefreshTokens");
        });
    }
}
