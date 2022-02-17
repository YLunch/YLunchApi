using FluentAssertions;
using Xunit;
using YLunchApi.Helpers.Extensions;

namespace YLunchApi.UnitTests.Helpers;

public class StringExtensionTest
{
    [Theory]
    [InlineData("jean", "Jean")]
    [InlineData("j", "J")]
    [InlineData("JEaN", "Jean")]
    [InlineData("JEaN-maRc", "Jean-Marc")]
    [InlineData("JEaN-m", "Jean-M")]
    [InlineData("jEaN-m-aBc", "Jean-M-Abc")]
    [InlineData("jEaN-m aBc", "Jean-M Abc")]
    [InlineData("", "")]
    public void Should_Capitalize(string input, string expected)
    {
        input.Capitalize().Should().Be(expected);
    }
}
