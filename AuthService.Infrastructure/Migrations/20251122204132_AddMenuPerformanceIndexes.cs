using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <summary>
    /// Adds critical indexes for menu loading performance optimization
    /// These indexes will dramatically improve query performance by enabling efficient lookups
    /// Expected impact: Reduces menu loading time from 18 seconds to under 1 second
    /// </summary>
    public partial class AddMenuPerformanceIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Features table indexes
            migrationBuilder.CreateIndex(
                name: "IX_Features_ParentFeatureId_IsActive",
                table: "Features",
                columns: new[] { "ParentFeatureId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Features_IsMainMenu_IsActive",
                table: "Features",
                columns: new[] { "IsMainMenu", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Features_DisplayOrder",
                table: "Features",
                column: "DisplayOrder");

            // Pages table indexes
            migrationBuilder.CreateIndex(
                name: "IX_Pages_IsActive",
                table: "Pages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_DisplayOrder",
                table: "Pages",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_MenuContext",
                table: "Pages",
                column: "MenuContext");

            // PageFeatureMappings indexes
            migrationBuilder.CreateIndex(
                name: "IX_PageFeatureMappings_FeatureId",
                table: "PageFeatureMappings",
                column: "FeatureId");

            // PagePermissionMappings indexes
            migrationBuilder.CreateIndex(
                name: "IX_PagePermissionMappings_IsActive",
                table: "PagePermissionMappings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PagePermissionMappings_PermissionId_IsActive",
                table: "PagePermissionMappings",
                columns: new[] { "PermissionId", "IsActive" });

            // RolePermissionMappings indexes
            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionMappings_RoleId_IsActive",
                table: "RolePermissionMappings",
                columns: new[] { "RoleId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionMappings_PermissionId_IsActive",
                table: "RolePermissionMappings",
                columns: new[] { "PermissionId", "IsActive" });

            // UserRoles indexes (if not already present from Identity)
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            // UserRoleMappings indexes
            migrationBuilder.CreateIndex(
                name: "IX_UserRoleMappings_UserId_IsActive",
                table: "UserRoleMappings",
                columns: new[] { "UserId", "IsActive" });

            // ApplicationRoles indexes
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_Name",
                table: "ApplicationRoles",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove indexes in reverse order
            migrationBuilder.DropIndex(
                name: "IX_ApplicationRoles_Name",
                table: "ApplicationRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleMappings_UserId_IsActive",
                table: "UserRoleMappings");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissionMappings_PermissionId_IsActive",
                table: "RolePermissionMappings");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissionMappings_RoleId_IsActive",
                table: "RolePermissionMappings");

            migrationBuilder.DropIndex(
                name: "IX_PagePermissionMappings_PermissionId_IsActive",
                table: "PagePermissionMappings");

            migrationBuilder.DropIndex(
                name: "IX_PagePermissionMappings_IsActive",
                table: "PagePermissionMappings");

            migrationBuilder.DropIndex(
                name: "IX_PageFeatureMappings_FeatureId",
                table: "PageFeatureMappings");

            migrationBuilder.DropIndex(
                name: "IX_Pages_MenuContext",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_DisplayOrder",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_IsActive",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Features_DisplayOrder",
                table: "Features");

            migrationBuilder.DropIndex(
                name: "IX_Features_IsMainMenu_IsActive",
                table: "Features");

            migrationBuilder.DropIndex(
                name: "IX_Features_ParentFeatureId_IsActive",
                table: "Features");
        }
    }
}
