using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Domain.UserAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;

    public UserRepository(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _userManager = userManager;
        _context = context;
        _roleManager = roleManager;
    }

    public async Task Create(User user, string password, string role)
    {
        var result = await _userManager.CreateAsync(user, password);
        var success = result.Succeeded;

        if (!await _roleManager.RoleExistsAsync(Roles.SuperAdmin))
            success &= (await _roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin))).Succeeded;
        if (!await _roleManager.RoleExistsAsync(Roles.RestaurantAdmin))
            success &= (await _roleManager.CreateAsync(new IdentityRole(Roles.RestaurantAdmin))).Succeeded;
        if (!await _roleManager.RoleExistsAsync(Roles.Customer))
            success &= (await _roleManager.CreateAsync(new IdentityRole(Roles.Customer))).Succeeded;

        var roleExists = await _roleManager.RoleExistsAsync(role);
        if (!roleExists) throw new EntityNotFoundException("Role not found");

        if (success)
            success &= (await _userManager.AddToRoleAsync(user, role)).Succeeded;

        if (!success) throw new UserRegistrationException();
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetByEmailAndPassword(string email, string password)
    {
        var user = await GetByEmail(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password)) return null;

        return user;
    }

    public async Task<List<string>> GetUserRoles(User user)
    {
        var iListRoles = await _userManager.GetRolesAsync(user);
        return iListRoles.ToList();
    }

    public async Task<User?> GetById(string id)
    {
        return await _context.Users.FindAsync(id);
    }
}
