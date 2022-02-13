using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Authentication.Models.Dto;

public class TokenUpdateDto
{
    [Required] public string AccessToken { get; set; } = null!;
    [Required] public string RefreshToken { get; set; } = null!;
}
