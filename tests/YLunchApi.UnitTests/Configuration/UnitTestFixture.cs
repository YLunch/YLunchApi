using Xunit;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.UnitTests.Configuration;

public class UnitTestFixture : IClassFixture<UnitTestFixtureBase>
{
    protected readonly UnitTestFixtureBase Fixture;

    protected UnitTestFixtureBase AnonymousUserFixture => Fixture.InitFixture();
    protected UnitTestFixtureBase AuthenticatedRestaurantAdminFixture => Fixture.InitFixture(configuration =>
        configuration.AccessToken = TokenMocks.ValidRestaurantAdminAccessToken);
    protected UnitTestFixtureBase AuthenticatedCustomerFixture => Fixture.InitFixture(configuration =>
        configuration.AccessToken = TokenMocks.ValidCustomerAccessToken);

    protected UnitTestFixture(UnitTestFixtureBase fixture)
    {
        Fixture = fixture;
    }
}
