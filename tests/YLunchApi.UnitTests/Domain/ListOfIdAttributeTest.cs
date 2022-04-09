using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

namespace YLunchApi.UnitTests.Domain;

public class ListOfIdAttributeTest
{
    private readonly ListOfIdAttribute _attribute = new();

    [Fact]
    public void Null_Should_Be_Valid()
    {
        // Arrange & Act & Assert
        _attribute.IsValid(null).Should().BeTrue();
    }

    [Fact]
    public void List_Of_Id_Should_Be_Valid()
    {
        // Arrange
        var listOfId = new List<string>
        {
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        };

        // Act & Assert
        _attribute.IsValid(listOfId).Should().BeTrue();
    }

    [Fact]
    public void List_Of_Id_Should_Be_Invalid()
    {
        // Arrange
        var listOfId = new List<string>
        {
            "BadId",
            Guid.NewGuid().ToString()
        };

        // Act & Assert
        _attribute.IsValid(listOfId).Should().BeFalse();
    }

    [Fact]
    public void List_Of_NonString_Should_Be_Invalid()
    {
        // Arrange
        var listOfId = new List<int>
        {
            1,
            2
        };

        // Act & Assert
        _attribute.IsValid(listOfId).Should().BeFalse();
    }

    [Fact]
    public void FormatErrorMessage_Should_Return_Right_Message()
    {
        // Arrange & Act
        var errorMessage = _attribute.FormatErrorMessage("");

        // Assert
        errorMessage.Should().Be("Must be a list of id which match Guid regular expression.");
    }
}
