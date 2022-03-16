using System;
using Xunit;

namespace YLunchApi.UnitTests.Core.Configuration;

public class UnitTestFixture : IClassFixture<UnitTestFixtureBase>
{
    protected readonly UnitTestFixtureBase Fixture;

    protected UnitTestFixture(UnitTestFixtureBase fixture)
    {
        Fixture = fixture;
        fixture.DatabaseId = Guid.NewGuid().ToString();
    }
}
