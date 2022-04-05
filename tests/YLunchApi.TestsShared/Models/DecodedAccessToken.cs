using YLunchApi.Authentication.Models;

namespace YLunchApi.TestsShared.Models;

public class DecodedAccessToken : ApplicationSecurityToken
{
    public string? RefreshToken { get; }

    public DecodedAccessToken(string accessToken, string? refreshToken = null) : base(accessToken)
    {
        RefreshToken = refreshToken;
    }
}
