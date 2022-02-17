using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.UnitTests.Authentication;

public class ApplicationSecurityTokenTest
{
    [Fact]
    public void Should_Be_Valid()
    {
        // Arrange
        var user = UserMocks.RestaurantAdminUserReadDto("095f0658-8192-442d-9a3a-2b78c4268aa3");
        const string token = TokenMocks.ValidRestaurantAdminAccessToken;
        var expected = new ApplicationSecurityToken(token);

        // Act
        var actual = new ApplicationSecurityToken(token);
        expected.Should().BeEquivalentTo(actual, options =>
            options.Excluding(x => x.UserId)
        );
        actual.UserId.Should().Be(user.Id);
        actual.UserEmail.Should().Be(user.Email);
        actual.UserRoles.Should().BeEquivalentTo(new List<string> { Roles.RestaurantAdmin });
    }
}
