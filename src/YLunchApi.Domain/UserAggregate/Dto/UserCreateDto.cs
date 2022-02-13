using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.UserAggregate.Dto;

public abstract class UserCreateDto
{
    [Required]
    [RegularExpression(
        @"^[a-z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?)*\.[a-z]{2,20}$",
        ErrorMessage = "Email is invalid. Should use right format with no uppercase.")]
    public virtual string Email { get; set; } = null!;

    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[.@$!%*?&])[A-Za-z\d.@$!%*?&]{8,}$",
        ErrorMessage =
            "Password is not allowed. Must contain at least 8 characters, 1 lowercase letter, 1 uppercase letter, 1 special character and 1 number.")]
    public string Password { get; set; } = null!;

    [Required]
    [RegularExpression(
        @"^0[6-7][0-9]{8}$",
        ErrorMessage = "PhoneNumber is not allowed. Example: 0612345678.")]
    public string PhoneNumber { get; set; } = null!;

    [RegularExpression(
        @"^(:?[^\W0-9]{2,20}[ -]*[^\W0-9]{2,20})+$",
        ErrorMessage = "Firstname is not allowed. Should contain only letters and '-'")]
    [Required]
    public string Firstname { get; set; } = null!;

    [RegularExpression(
        @"^(:?[^\W0-9]{2,20}[ -]*[^\W0-9]{2,20})+$",
        ErrorMessage = "Lastname is not allowed. Should contain only letters and '-'")]
    [Required]
    public string Lastname { get; set; } = null!;
}
