namespace AuthService.Infrastructure.Persistence;

public class CommandDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, ICommandDbContext
{
    public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options)
    {
    }

    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserRefreshToken> RefreshTokens => Set<UserRefreshToken>();
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
        });

        builder.Entity<IdentityUserRole<Guid>>(b =>
        {
            b.ToTable("UserRoles");
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

        // Configure UserAddress
        builder.Entity<UserAddress>(b =>
        {
            b.ToTable("UserAddresses");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure UserRefreshToken
        builder.Entity<UserRefreshToken>(b =>
        {
            b.ToTable("UserRefreshTokens");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure Department
        builder.Entity<Department>(b =>
        {
            b.ToTable("Departments");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Permission
        builder.Entity<Permission>(b =>
        {
            b.ToTable("Permissions");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Feature
        builder.Entity<Feature>(b =>
        {
            b.ToTable("Features");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.Icon).HasMaxLength(100);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(e => e.Name).IsUnique();
            
            b.HasOne(f => f.ParentFeature)
                .WithMany(f => f.SubFeatures)
                .HasForeignKey(f => f.ParentFeatureId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Page
        builder.Entity<Page>(b =>
        {
            b.ToTable("Pages");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Url).IsRequired().HasMaxLength(500);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(e => e.Name).IsUnique();
        });

        // Configure RolePermissionMapping
        builder.Entity<RolePermissionMapping>(b =>
        {
            b.ToTable("RolePermissionMappings");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(rpm => rpm.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rpm => rpm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(rpm => rpm.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rpm => rpm.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(rpm => new { rpm.RoleId, rpm.PermissionId }).IsUnique();
        });

        // Configure PagePermissionMapping
        builder.Entity<PagePermissionMapping>(b =>
        {
            b.ToTable("PagePermissionMappings");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(ppm => ppm.Page)
                .WithMany(p => p.PagePermissions)
                .HasForeignKey(ppm => ppm.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ppm => ppm.Permission)
                .WithMany(p => p.PagePermissions)
                .HasForeignKey(ppm => ppm.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(ppm => new { ppm.PageId, ppm.PermissionId }).IsUnique();
        });

        // Configure PageFeatureMapping
        builder.Entity<PageFeatureMapping>(b =>
        {
            b.ToTable("PageFeatureMappings");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(pfm => pfm.Page)
                .WithMany(p => p.PageFeatures)
                .HasForeignKey(pfm => pfm.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(pfm => pfm.Feature)
                .WithMany(f => f.PageFeatures)
                .HasForeignKey(pfm => pfm.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(pfm => new { pfm.PageId, pfm.FeatureId }).IsUnique();
        });

        // Configure RoleHierarchy
        builder.Entity<RoleHierarchy>(b =>
        {
            b.ToTable("RoleHierarchies");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

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

        // Configure UserRoleMapping
        builder.Entity<UserRoleMapping>(b =>
        {
            b.ToTable("UserRoleMappings");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.AssignedByEmail).IsRequired().HasMaxLength(256);
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

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
        });

        // Configure RoleDepartmentMapping
        builder.Entity<RoleDepartmentMapping>(b =>
        {
            b.ToTable("RoleDepartmentMappings");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(rdm => rdm.Role)
                .WithMany()
                .HasForeignKey(rdm => rdm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(rdm => rdm.Department)
                .WithMany()
                .HasForeignKey(rdm => rdm.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(rdm => new { rdm.RoleId, rdm.DepartmentId }).IsUnique();
        });
    }
}
