using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Domain.UserAggregate.Dto;

public class UserReadDto : EntityReadDto
{
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public List<string> Roles { get; set; } = null!;

    public UserReadDto()
    {
    }

    public UserReadDto(User user, List<string> roles)
    {
        Id = user.Id;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        Firstname = user.Firstname;
        Lastname = user.Lastname;
        Roles = roles;
    }
}
