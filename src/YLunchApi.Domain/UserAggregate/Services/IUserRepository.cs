using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Domain.UserAggregate.Services;

public interface IUserRepository
{
    Task CreateUser(User user, string password, string role);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserByEmailAndPassword(string email, string password);
    Task<List<string>> GetUserRoles(User user);
    Task<User?> GetUserById(string id);
}
