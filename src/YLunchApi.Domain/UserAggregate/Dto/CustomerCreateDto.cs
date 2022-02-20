using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.UserAggregate.Dto;

public class CustomerCreateDto : UserCreateDto
{
    [Required]
    [EmailAddress]
    [RegularExpression(
        @"^(:?[^\W0-9]{2,20}(?:[-](?:[^\W0-9]{2,20})+)*)\.(:?[^\W0-9]{2,20}(?:[-](?:[^\W0-9]{2,20})+)*)@ynov.com$",
        ErrorMessage = "Email is invalid. You must provide your Ynov email")]
    public override string Email { get; set; } = null!;
}
