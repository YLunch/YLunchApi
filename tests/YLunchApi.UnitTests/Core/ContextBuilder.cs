using System;
using Microsoft.EntityFrameworkCore;
using YLunchApi.Infrastructure.Database;

namespace YLunchApi.UnitTests.Core;

public static class ContextBuilder
{
    public static ApplicationDbContext BuildContext(string? fixtureConfigurationDatabaseId = null)
    {
        var databaseId = fixtureConfigurationDatabaseId ?? Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                      .UseInMemoryDatabase($"YLunchDatabaseForUnitTests-{databaseId}")
                      .Options;
        return new ApplicationDbContext(options);
    }
}
