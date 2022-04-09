using FluentAssertions;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Authentication.Utils;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.TestsShared.Models;

namespace YLunchApi.UnitTests.Helpers;

public class DecodedTokensTest
{
    [Fact]
    public void DecodedTokens_Should_Be_Correct()
    {
        // Arrange
        var refreshToken = RandomUtils.GetRandomKey();

        // Act
        var decodedTokens = new DecodedTokens(TokenMocks.ValidCustomerAccessToken, refreshToken);

        // Assert
        decodedTokens.Should().BeEquivalentTo(new ApplicationSecurityToken(TokenMocks.ValidCustomerAccessToken),
            options => options.ExcludingMissingMembers());
        decodedTokens.RefreshToken.Should().Be(refreshToken);
    }
}
