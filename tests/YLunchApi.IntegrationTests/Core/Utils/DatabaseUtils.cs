using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YLunchApi.Infrastructure.Database;

namespace YLunchApi.IntegrationTests.Core.Utils;

public static class DatabaseUtils
{
    public static async Task InitializeDbForTests(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        // Database fulfillment...
        await context.SaveChangesAsync();
    }

    [ExcludeFromCodeCoverage]
    public static async Task ReinitializeDbForTests(ApplicationDbContext context)
    {
        await context.Database.EnsureDeletedAsync();
        await InitializeDbForTests(context);
    }
}
