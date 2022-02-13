using System.Diagnostics.CodeAnalysis;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Authentication.Models;

public class RefreshToken
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string JwtId { get; set; } = null!;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime ExpirationDateTime { get; set; }

    [ExcludeFromCodeCoverage] public virtual User? User { get; set; }
}
