using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Infrastructure.Database;
using YLunchApi.UnitTests.Core.Utils;

namespace YLunchApi.UnitTests.Core.Mocks;

public class UserManagerMock : UserManager<User>
{
    private readonly ApplicationDbContext _context;

    public UserManagerMock(ApplicationDbContext context) : base(
        new Mock<IUserStore<User>>().Object,
        new Mock<IOptions<IdentityOptions>>().Object,
        new Mock<IPasswordHasher<User>>().Object,
        Array.Empty<IUserValidator<User>>(),
        Array.Empty<IPasswordValidator<User>>(),
        new Mock<ILookupNormalizer>().Object,
        new Mock<IdentityErrorDescriber>().Object,
        new Mock<IServiceProvider>().Object,
        new Mock<ILogger<UserManager<User>>>().Object)
    {
        _context = context;
    }

    public override async Task<IdentityResult> CreateAsync(User user, string password)
    {
        user.PasswordHash = HashUtils.HashValue(password);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return IdentityResult.Success;
    }

    public override async Task<bool> CheckPasswordAsync(User user, string password)
    {
        var userDb = await _context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);
        return userDb!.PasswordHash == HashUtils.HashValue(password);
    }

    public override async Task<IList<string>> GetRolesAsync(User user)
    {
        var userRolesId = await _context.UserRoles
                                        .Where(x => x.UserId == user.Id)
                                        .Select(x => x.RoleId)
                                        .ToListAsync();
        var roles = await _context.Roles
                                  .Where(r => userRolesId.Contains(r.Id))
                                  .Select(x => x.Name)
                                  .ToListAsync();
        return roles;
    }

    public override async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        var identityRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role);

        await _context.UserRoles.AddAsync(new IdentityUserRole<string>
            { UserId = user.Id, RoleId = identityRole!.Id });
        await _context.SaveChangesAsync();
        return IdentityResult.Success;
    }
}
