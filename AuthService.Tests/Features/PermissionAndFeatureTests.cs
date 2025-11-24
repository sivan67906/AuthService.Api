using AuthService.Application.Features.Feature.CreateFeature;
using AuthService.Application.Features.Permission.CreatePermission;

namespace AuthService.Tests.Features;

public class PermissionTests
{
    [Fact]
    public void CreatePermissionCommand_ShouldHaveCorrectProperties()
    {
        // Arrange
        var name = "ViewReports";
        var description = "View Reports Permission";

        // Act
        var command = new CreatePermissionCommand(name, description);

        // Assert
        Assert.Equal(name, command.Name);
        Assert.Equal(description, command.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreatePermissionCommandValidator_ShouldFailForEmptyName(string? name)
    {
        // Arrange
        var validator = new CreatePermissionCommandValidator();
        var command = new CreatePermissionCommand(name!, null);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreatePermissionCommandValidator_ShouldPassForValidData()
    {
        // Arrange
        var validator = new CreatePermissionCommandValidator();
        var command = new CreatePermissionCommand("ViewReports", "View Reports Permission");

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}

public class FeatureTests
{
    [Fact]
    public void CreateFeatureCommand_ShouldHaveCorrectProperties()
    {
        // Arrange
        var name = "Reporting";
        var description = "Reporting Feature";

        // Act
        var command = new CreateFeatureCommand(name, description);

        // Assert
        Assert.Equal(name, command.Name);
        Assert.Equal(description, command.Description);
    }

    [Fact]
    public void CreateFeatureCommandValidator_ShouldPassForValidData()
    {
        // Arrange
        var validator = new CreateFeatureCommandValidator();
        var command = new CreateFeatureCommand("Reporting", "Reporting Feature");

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
