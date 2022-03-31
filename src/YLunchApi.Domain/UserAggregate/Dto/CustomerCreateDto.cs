using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.UserAggregate.Dto;

public class CustomerCreateDto : UserCreateDto
{
    [Required]
    [EmailAddress]
    [RegularExpression(
        @"^(:?[a-z]{1,50}(?:[_\-](?:[a-z]{1,50})+)*)\.(:?[a-z]{1,50}(?:[_\-](?:[a-z]{1,50})+)*)@ynov.com$",
        ErrorMessage = "Email is invalid. It should be a lowercase Ynov email format and could contain '.', '-' and/or '_' characters.")]
    public override string Email { get; set; } = null!;
}
