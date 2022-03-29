using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using Moq;
using Xunit;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Configuration;

namespace YLunchApi.UnitTests.Domain;

public class InFutureOrNullAttributeTest : UnitTestFixture
{
    private readonly InFutureOrNullAttribute _attribute = new();

    public InFutureOrNullAttributeTest(UnitTestFixtureBase fixture) : base(fixture)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTimeMocks.Monday20220321T1000Utc);
        Fixture.InitFixture(configuration => configuration.DateTimeProvider = dateTimeProviderMock.Object);
    }

    [Fact]
    public void Null_Should_Be_Invalid()
    {
        // Arrange & Act & Assert
        _attribute.IsValid(null, new ValidationContext("")).Should().BeNull();
    }

    [Fact]
    public void DateTime_Should_Be_Valid()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc.AddMinutes(1);
        var validationContext = new ValidationContext(dateTime);
        validationContext.InitializeServiceProvider(_ => Fixture.GetImplementationFromService<IDateTimeProvider>());

        // Act & Assert
        _attribute.IsValid(dateTime, validationContext).Should().BeNull();
    }

    [Fact]
    public void DateTime_Should_Be_Invalid()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc.AddMinutes(-1);
        var validationContext = new ValidationContext(dateTime);
        validationContext.InitializeServiceProvider(_ => Fixture.GetImplementationFromService<IDateTimeProvider>());

        // Act & Assert
        _attribute.IsValid(dateTime, validationContext).Should().NotBeNull();
    }

    [Fact]
    public void FormatErrorMessage_Should_Return_Right_Message()
    {
        // Arrange & Act
        var errorMessage = _attribute.FormatErrorMessage("DateTime");

        // Assert
        errorMessage.Should().Be("DateTime must be in future if present.");
    }
}

[ExcludeFromCodeCoverage]
internal static class InFutureOrNullAttributeExtension
{
    public static ValidationResult? IsValid(this InFutureOrNullAttribute attribute, object? value, ValidationContext validationContext)
    {
        var isValidMethod = attribute.GetType()
                                     .GetMethod("IsValid", BindingFlags.NonPublic | BindingFlags.Instance);
        if (isValidMethod == null)
        {
            throw new InvalidOperationException("IsValidMethod of InFutureOrNullAttribute is null");
        }

        return isValidMethod.Invoke(attribute, new[] { value, validationContext }) as ValidationResult;
    }
}
