using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Helpers.Extensions;

namespace YLunchApi.Domain.UserAggregate.Models;

public class User : IdentityUser
{
    [ExcludeFromCodeCoverage]
    public User()
    {
    }

    public User(UserCreateDto userCreateDto)
    {
        Id = Guid.NewGuid().ToString();
        UserName = userCreateDto.Email.ToLower();
        Email = userCreateDto.Email.ToLower();
        PhoneNumber = userCreateDto.PhoneNumber;
        Firstname = userCreateDto.Firstname.Capitalize();
        Lastname = userCreateDto.Lastname.Capitalize();
    }

    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
}
