using YLunchApi.Helpers.Extensions;

namespace YLunchApi.Domain.UserAggregate.Models;

public sealed class AuthenticatedUser : User
{
    public AuthenticatedUser(User user, List<string> roles)
    {
        Id = user.Id;
        UserName = user.Email.ToLower();
        Email = user.Email.ToLower();
        PhoneNumber = user.PhoneNumber;
        Firstname = user.Firstname.Capitalize();
        Lastname = user.Lastname.Capitalize();
        Roles = roles;
    }

    public List<string> Roles { get; set; }
}
