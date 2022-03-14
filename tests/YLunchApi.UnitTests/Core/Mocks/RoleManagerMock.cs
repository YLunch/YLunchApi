using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using YLunchApi.Infrastructure.Database;

namespace YLunchApi.UnitTests.Core.Mocks;

public class RoleManagerMock : RoleManager<IdentityRole>
{
    private readonly ApplicationDbContext _context;

    public RoleManagerMock(ApplicationDbContext context) : base(
        new Mock<IRoleStore<IdentityRole>>().Object,
        Array.Empty<IRoleValidator<IdentityRole>>(),
        new Mock<ILookupNormalizer>().Object,
        new Mock<IdentityErrorDescriber>().Object,
        new Mock<ILogger<RoleManager<IdentityRole>>>().Object)
    {
        _context = context;
    }

    public override async Task<IdentityResult> CreateAsync(IdentityRole role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
        return IdentityResult.Success;
    }

    public override async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _context.Roles.AnyAsync(x => x.Name == roleName);
    }
}
