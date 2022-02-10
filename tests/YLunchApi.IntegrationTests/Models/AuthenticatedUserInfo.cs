using YLunchApi.Authentication.Models;

namespace YLunchApi.IntegrationTests.Models;

public class AuthenticatedUserInfo : ApplicationSecurityToken
{
    public string RefreshToken { get; }

    public AuthenticatedUserInfo(string accessToken, string refreshToken) : base(accessToken)
    {
        RefreshToken = refreshToken;
    }
}
