using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Domain.UserAggregate.Services;

public interface IUserRepository
{
    Task Create(User user, string password, string role);
    Task<User?> GetByEmail(string email);
    Task<User?> GetByEmailAndPassword(string email, string password);
    Task<List<string>> GetUserRoles(User user);
    Task<User?> GetById(string id);
}
