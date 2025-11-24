using AuthService.Domain.Interfaces;

namespace AuthService.Infrastructure.Persistence;

public class QueryDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IQueryDbContext
{
    public QueryDbContext(DbContextOptions<QueryDbContext> options) : base(options)
    {
    }

    // DbSets remain the same...
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<RolePermissionMapping> RolePermissionMappings => Set<RolePermissionMapping>();
    public DbSet<PagePermissionMapping> PagePermissionMappings => Set<PagePermissionMapping>();
    public DbSet<PageFeatureMapping> PageFeatureMappings => Set<PageFeatureMapping>();
    public DbSet<RoleHierarchy> RoleHierarchies => Set<RoleHierarchy>();
    public DbSet<UserRoleMapping> UserRoleMappings => Set<UserRoleMapping>();
    public DbSet<RoleDepartmentMapping> RoleDepartmentMappings => Set<RoleDepartmentMapping>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserRefreshToken> RefreshTokens => Set<UserRefreshToken>();
    public new DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();
    public new DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public new DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity tables
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("ApplicationUsers");
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("ApplicationRoles");

            b.HasOne(r => r.Department)
                .WithMany(d => d.Roles)
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Performance index for role name lookups
            b.HasIndex(r => r.Name);
        });

        builder.Entity<IdentityUserRole<Guid>>(b =>
        {
            b.ToTable("UserRoles");
            
            // Performance index for user role lookups
            b.HasIndex(ur => ur.UserId);
        });

        builder.Entity<IdentityUserLogin<Guid>>(b =>
        {
            b.ToTable("UserLogins");
        });

        builder.Entity<IdentityUserToken<Guid>>(b =>
        {
            b.ToTable("UserTokens");
        });

        builder.Entity<IdentityRoleClaim<Guid>>(b =>
        {
            b.ToTable("RoleClaims");
        });

        builder.Entity<IdentityUserClaim<Guid>>(b =>
        {
            b.ToTable("UserClaims");
        });

        // Configure UserAddress - PostgreSQL syntax
        builder.Entity<UserAddress>(b =>
        {
            b.ToTable("UserAddresses");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");
        });

        // Configure UserRefreshToken - PostgreSQL syntax
        builder.Entity<UserRefreshToken>(b =>
        {
            b.ToTable("UserRefreshTokens");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");
        });

        // Configure Department - PostgreSQL syntax
        builder.Entity<Department>(b =>
        {
            b.ToTable("Departments");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");
            b.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Permission - PostgreSQL syntax
        builder.Entity<Permission>(b =>
        {
            b.ToTable("Permissions");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");
            b.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Feature - PostgreSQL syntax with performance indexes
        builder.Entity<Feature>(b =>
        {
            b.ToTable("Features");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.Icon).HasMaxLength(100);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");
            b.HasIndex(e => e.Name).IsUnique();

            b.HasOne(f => f.ParentFeature)
                .WithMany(f => f.SubFeatures)
                .HasForeignKey(f => f.ParentFeatureId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // PERFORMANCE INDEXES for menu loading
            b.HasIndex(f => new { f.ParentFeatureId, f.IsActive });
            b.HasIndex(f => new { f.IsMainMenu, f.IsActive });
            b.HasIndex(f => f.DisplayOrder);
        });

        // Configure Page - PostgreSQL syntax with performance indexes
        builder.Entity<Page>(b =>
        {
            b.ToTable("Pages");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Url).IsRequired().HasMaxLength(500);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");
            b.HasIndex(e => e.Name).IsUnique();
            
            // PERFORMANCE INDEXES for menu loading
            b.HasIndex(p => p.IsActive);
            b.HasIndex(p => p.DisplayOrder);
            b.HasIndex(p => p.MenuContext);
        });

        // Configure RolePermissionMapping - PostgreSQL syntax with performance indexes
        builder.Entity<RolePermissionMapping>(b =>
        {
            b.ToTable("RolePermissionMappings");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");

            b.HasOne(rpm => rpm.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rpm => rpm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(rpm => rpm.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rpm => rpm.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(rpm => new { rpm.RoleId, rpm.PermissionId }).IsUnique();
            
            // PERFORMANCE INDEXES for permission lookups
            b.HasIndex(rpm => new { rpm.RoleId, rpm.IsActive });
            b.HasIndex(rpm => new { rpm.PermissionId, rpm.IsActive });
        });

        // Configure PagePermissionMapping - PostgreSQL syntax with performance indexes
        builder.Entity<PagePermissionMapping>(b =>
        {
            b.ToTable("PagePermissionMappings");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");

            b.HasOne(ppm => ppm.Page)
                .WithMany(p => p.PagePermissions)
                .HasForeignKey(ppm => ppm.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ppm => ppm.Permission)
                .WithMany(p => p.PagePermissions)
                .HasForeignKey(ppm => ppm.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(ppm => new { ppm.PageId, ppm.PermissionId }).IsUnique();
            
            // PERFORMANCE INDEXES for permission checks
            b.HasIndex(ppm => ppm.IsActive);
            b.HasIndex(ppm => new { ppm.PermissionId, ppm.IsActive });
        });

        // Configure PageFeatureMapping - PostgreSQL syntax with performance indexes
        builder.Entity<PageFeatureMapping>(b =>
        {
            b.ToTable("PageFeatureMappings");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");

            b.HasOne(pfm => pfm.Page)
                .WithMany(p => p.PageFeatures)
                .HasForeignKey(pfm => pfm.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(pfm => pfm.Feature)
                .WithMany(f => f.PageFeatures)
                .HasForeignKey(pfm => pfm.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(pfm => new { pfm.PageId, pfm.FeatureId }).IsUnique();
            
            // PERFORMANCE INDEX for feature-to-page lookups
            b.HasIndex(pfm => pfm.FeatureId);
        });

        // Configure RoleHierarchy - PostgreSQL syntax
        builder.Entity<RoleHierarchy>(b =>
        {
            b.ToTable("RoleHierarchies");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");

            b.HasOne(rh => rh.ParentRole)
                .WithMany()
                .HasForeignKey(rh => rh.ParentRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(rh => rh.ChildRole)
                .WithMany()
                .HasForeignKey(rh => rh.ChildRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(rh => new { rh.ParentRoleId, rh.ChildRoleId }).IsUnique();
        });

        // Configure UserRoleMapping - PostgreSQL syntax with performance indexes
        builder.Entity<UserRoleMapping>(b =>
        {
            b.ToTable("UserRoleMappings");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.AssignedByEmail).IsRequired().HasMaxLength(256);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");

            b.HasOne(urm => urm.User)
                .WithMany()
                .HasForeignKey(urm => urm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(urm => urm.Role)
                .WithMany()
                .HasForeignKey(urm => urm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(urm => urm.Department)
                .WithMany()
                .HasForeignKey(urm => urm.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(urm => new { urm.UserId, urm.RoleId, urm.DepartmentId });
            
            // PERFORMANCE INDEX for user-specific queries
            b.HasIndex(urm => new { urm.UserId, urm.IsActive });
        });
    }
}
