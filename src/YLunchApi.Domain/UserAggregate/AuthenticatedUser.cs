using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.UserAggregate;

public class AuthenticatedUser : User
{
    public List<string> Roles { get; set; }

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
}
