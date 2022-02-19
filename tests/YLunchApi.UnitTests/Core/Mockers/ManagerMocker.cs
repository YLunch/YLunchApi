using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Infrastructure.Database;

namespace YLunchApi.UnitTests.Core.Mockers;

public static class ManagerMocker
{
    public static Mock<RoleManager<IdentityRole>> GetRoleManagerMock(ApplicationDbContext context)
    {
        var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            new Mock<IRoleStore<IdentityRole>>().Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

        roleManagerMock.Setup(x => x.DeleteAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
        roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success)
                       .Callback<IdentityRole>(async x =>
                       {
                           await context.Roles.AddAsync(x);
                           await context.SaveChangesAsync();
                       });
        roleManagerMock.Setup(x => x.UpdateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
        roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
                       .Returns<string>(async roleName =>
                           await context.Roles.AnyAsync(x => x.Name == roleName)
                       );

        return roleManagerMock;
    }

    public static Mock<UserManager<User>> GetUserManagerMock(ApplicationDbContext context)
    {
        var userManagerMock = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<User>>().Object,
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<User>>>().Object);


        userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success)
                       .Callback<User, string>(async (x, password) =>
                       {
                           x.PasswordHash = HashUtils.HashValue(password);
                           await context.Users.AddAsync(x);
                           await context.SaveChangesAsync();
                       });
        userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success)
                       .Callback<User, string>(async (x, role) =>
                       {
                           var identityRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == role);

                           await context.UserRoles.AddAsync(new IdentityUserRole<string>
                               { UserId = x.Id, RoleId = identityRole!.Id });
                           await context.SaveChangesAsync();
                       });
        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                       .Returns<User>(async user =>
                       {
                           var userRolesId = await context.UserRoles
                                                          .Where(x => x.UserId == user.Id)
                                                          .Select(x => x.RoleId)
                                                          .ToListAsync();
                           var roles = await context.Roles
                                                    .Where(r => userRolesId.Contains(r.Id))
                                                    .Select(x => x.Name)
                                                    .ToListAsync();
                           return roles;
                       });
        userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
                       .Returns<User, string>(async (user, password) =>
                       {
                           var userDb = await context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);
                           return userDb!.PasswordHash == HashUtils.HashValue(password);
                       });

        return userManagerMock;
    }
}
