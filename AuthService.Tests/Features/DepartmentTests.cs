using AuthService.Application.Features.Department.CreateDepartment;

namespace AuthService.Tests.Features;

public class DepartmentTests
{
    [Fact]
    public void CreateDepartmentCommand_ShouldHaveCorrectProperties()
    {
        // Arrange
        var name = "Test Department";
        var description = "Test Description";

        // Act
        var command = new CreateDepartmentCommand(name, description);

        // Assert
        Assert.Equal(name, command.Name);
        Assert.Equal(description, command.Description);
    }

    [Fact]
    public void DepartmentDto_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Finance";
        var description = "Finance Department";
        var isActive = true;
        var createdAt = DateTime.UtcNow;
        DateTime? updatedAt = null;

        // Act
        var dto = new DepartmentDto(id, name, description, isActive, createdAt, updatedAt);

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(name, dto.Name);
        Assert.Equal(description, dto.Description);
        Assert.True(dto.IsActive);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Null(dto.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateDepartmentCommandValidator_ShouldFailForEmptyName(string? name)
    {
        // Arrange
        var validator = new CreateDepartmentCommandValidator();
        var command = new CreateDepartmentCommand(name!, null);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void CreateDepartmentCommandValidator_ShouldPassForValidData()
    {
        // Arrange
        var validator = new CreateDepartmentCommandValidator();
        var command = new CreateDepartmentCommand("Finance", "Finance Department");

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
