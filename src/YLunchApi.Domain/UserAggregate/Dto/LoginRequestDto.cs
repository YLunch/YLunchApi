using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.UserAggregate.Dto;

public class LoginRequestDto
{
    [RegularExpression(
        @"^[a-z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?)*\.[a-z]{2,20}$",
        ErrorMessage = "Email is invalid. Should use right format with no uppercase.")]
    [Required]
    public string Email { get; set; } = null!;

    [Required] public string Password { get; set; } = null!;
}
