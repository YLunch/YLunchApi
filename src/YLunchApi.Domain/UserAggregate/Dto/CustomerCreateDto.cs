using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.UserAggregate.Dto;

public class CustomerCreateDto : UserCreateDto
{
    [Required]
    [EmailAddress]
    [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@ynov.com$",
        ErrorMessage = "Email is invalid. You must provide your Ynov email")]
    public override string Email { get; set; } = null!;
}
