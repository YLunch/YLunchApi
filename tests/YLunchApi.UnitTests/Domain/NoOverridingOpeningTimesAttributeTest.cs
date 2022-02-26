using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

namespace YLunchApi.UnitTests.Domain;

public class NoOverridingOpeningTimesAttributeTest
{
    private readonly NoOverridingOpeningTimesAttribute _attribute;

    public NoOverridingOpeningTimesAttributeTest()
    {
        _attribute = new NoOverridingOpeningTimesAttribute();
    }

    [Fact]
    public void Null_Should_Be_Invalid()
    {
        // Arrange & Act & Assert
        _attribute.IsValid(null).Should().BeFalse();
    }

    [Fact]
    public void Opening_Times_Should_Be_Valid()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var openingTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = 2 * 60,
                DurationInMinutes = 3 * 60
            },
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = 6 * 60,
                DurationInMinutes = 3 * 60
            }
        };

        // Act & Assert
        _attribute.IsValid(openingTimes).Should().BeTrue();
    }

    [Fact]
    public void Opening_Times_Should_Be_Invalid()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var openingTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = 2 * 60,
                DurationInMinutes = 3 * 60
            },
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = 1 * 60,
                DurationInMinutes = 3 * 60
            }
        };

        // Act & Assert
        _attribute.IsValid(openingTimes).Should().BeFalse();
    }

    [Fact]
    public void FormatErrorMessage_Should_Return_Right_Message()
    {
        // Arrange & Act
        var errorMessage = _attribute.FormatErrorMessage("");

        // Assert
        errorMessage.Should().Be("Some opening times override others.");
    }
}
