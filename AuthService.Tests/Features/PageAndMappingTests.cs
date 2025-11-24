using AuthService.Application.Features.Page.CreatePage;
using AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;

namespace AuthService.Tests.Features;

public class PageTests
{
    [Fact]
    public void CreatePageCommand_ShouldHaveCorrectProperties()
    {
        // Arrange
        var name = "Dashboard";
        var url = "/dashboard";
        var description = "Main Dashboard";
        var displayOrder = 1;

        // Act
        var command = new CreatePageCommand(name, url, description, displayOrder);

        // Assert
        Assert.Equal(name, command.Name);
        Assert.Equal(url, command.Url);
        Assert.Equal(description, command.Description);
        Assert.Equal(displayOrder, command.DisplayOrder);
    }

    [Theory]
    [InlineData("", "/test")]
    [InlineData(null, "/test")]
    [InlineData("Test", "")]
    [InlineData("Test", null)]
    public void CreatePageCommandValidator_ShouldFailForEmptyRequiredFields(string? name, string? url)
    {
        // Arrange
        var validator = new CreatePageCommandValidator();
        var command = new CreatePageCommand(name!, url!, null, 1);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreatePageCommandValidator_ShouldPassForValidData()
    {
        // Arrange
        var validator = new CreatePageCommandValidator();
        var command = new CreatePageCommand("Dashboard", "/dashboard", "Main Dashboard", 1);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}

public class RolePermissionMappingTests
{
    [Fact]
    public void CreateRolePermissionMappingCommand_ShouldHaveCorrectProperties()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        var command = new CreateRolePermissionMappingCommand(roleId, permissionId);

        // Assert
        Assert.Equal(roleId, command.RoleId);
        Assert.Equal(permissionId, command.PermissionId);
    }

    [Fact]
    public void CreateRolePermissionMappingCommandValidator_ShouldFailForEmptyGuids()
    {
        // Arrange
        var validator = new CreateRolePermissionMappingCommandValidator();
        var command = new CreateRolePermissionMappingCommand(Guid.Empty, Guid.Empty);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateRolePermissionMappingCommandValidator_ShouldPassForValidData()
    {
        // Arrange
        var validator = new CreateRolePermissionMappingCommandValidator();
        var command = new CreateRolePermissionMappingCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
