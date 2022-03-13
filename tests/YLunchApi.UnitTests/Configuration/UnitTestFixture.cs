using Xunit;

namespace YLunchApi.UnitTests.Configuration;

public class UnitTestFixture : IClassFixture<UnitTestFixtureBase>
{
    protected readonly UnitTestFixtureBase Fixture;

    protected UnitTestFixture(UnitTestFixtureBase fixture)
    {
        Fixture = fixture;
    }
}
