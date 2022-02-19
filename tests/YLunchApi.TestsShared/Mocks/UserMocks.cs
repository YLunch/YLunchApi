using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.TestsShared.Mocks;

public static class UserMocks
{
    public static RestaurantAdminCreateDto RestaurantAdminCreateDto => new()
    {
        Email = "admin@restaurant.com",
        Firstname = "Jean-Marc",
        Lastname = "Dupont Henri",
        PhoneNumber = "0612345678",
        Password = "Password1234."
    };

    public static CustomerCreateDto CustomerCreateDto => new()
    {
        Email = "anne-marie.martin@ynov.com",
        Firstname = "Anne-Marie",
        Lastname = "Martin-Jacques",
        PhoneNumber = "0687654321",
        Password = "Password1234."
    };

    public static UserReadDto CustomerUserReadDto(string id)
    {
        return new UserReadDto
        {
            Id = id,
            Email = CustomerCreateDto.Email,
            PhoneNumber = CustomerCreateDto.PhoneNumber,
            Firstname = CustomerCreateDto.Firstname,
            Lastname = CustomerCreateDto.Lastname,
            Roles = new List<string> { Roles.Customer },
        };
    }

    public static UserReadDto RestaurantAdminUserReadDto(string id)
    {
        return new UserReadDto
        {
            Id = id,
            Email = RestaurantAdminCreateDto.Email,
            PhoneNumber = RestaurantAdminCreateDto.PhoneNumber,
            Firstname = RestaurantAdminCreateDto.Firstname,
            Lastname = RestaurantAdminCreateDto.Lastname,
            Roles = new List<string> { Roles.RestaurantAdmin },
        };
    }
}
