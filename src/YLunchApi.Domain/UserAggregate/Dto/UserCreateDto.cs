using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.UserAggregate.Dto;

public abstract class UserCreateDto
{
    [Required]
    [RegularExpression(
        @"^[a-z0-9._-]+@[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?)*\.[a-z]{2,20}$",
        ErrorMessage = "Email is invalid. It should be lowercase email format and could contain '.', '-' and/or '_' characters. Example: example@example.com.")]
    public virtual string Email { get; set; } = null!;

    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[.@$!%*?&])[A-Za-z\d.@$!%*?&]{8,}$",
        ErrorMessage =
            "Password is invalid. Must contain at least 8 characters, 1 lowercase letter, 1 uppercase letter, 1 special character and 1 number.")]
    public string Password { get; set; } = null!;

    [Required]
    [RegularExpression(
        @"^0[6-7][0-9]{8}$",
        ErrorMessage = "PhoneNumber is invalid. Example: '0612345678'.")]
    public string PhoneNumber { get; set; } = null!;

    [RegularExpression(
        @"^(:?[A-Za-zÀ-ÖØ-öø-ÿ]{1,50}(?:[ \-'](?:[A-Za-zÀ-ÖØ-öø-ÿ]{1,50})+)*)$",
        ErrorMessage = "Firstname is invalid. Should contain only letters and - or ' or space as separators.")]
    [Required]
    public string Firstname { get; set; } = null!;

    [RegularExpression(
        @"^(:?[A-Za-zÀ-ÖØ-öø-ÿ]{1,50}(?:[ \-'](?:[A-Za-zÀ-ÖØ-öø-ÿ]{1,50})+)*)$",
        ErrorMessage = "Lastname is invalid. Should contain only letters and - or ' or space as separators.")]
    [Required]
    public string Lastname { get; set; } = null!;
}
