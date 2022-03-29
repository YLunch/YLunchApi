using FluentAssertions;
using Xunit;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

namespace YLunchApi.UnitTests.Domain;

public class LowercaseAttributeTest
{
    private readonly LowercaseAttribute _attribute = new();

    [Fact]
    public void Null_Should_Be_Invalid()
    {
        // Arrange & Act & Assert
        _attribute.IsValid(null).Should().BeTrue();
    }

    [Fact]
    public void Opening_Times_Should_Be_Valid()
    {
        // Arrange & Act & Assert
        _attribute.IsValid("lowercase").Should().BeTrue();
    }

    [Fact]
    public void Opening_Times_Should_Be_Invalid()
    {
        // Arrange & Act & Assert
        _attribute.IsValid("Lowercase").Should().BeFalse();
    }

    [Fact]
    public void FormatErrorMessage_Should_Return_Right_Message()
    {
        // Arrange & Act
        var errorMessage = _attribute.FormatErrorMessage("");

        // Assert
        errorMessage.Should().Be("Must be lowercase.");
    }
}
