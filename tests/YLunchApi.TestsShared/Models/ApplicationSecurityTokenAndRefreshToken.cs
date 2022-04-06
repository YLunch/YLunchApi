using YLunchApi.Authentication.Models;

namespace YLunchApi.TestsShared.Models;

public class ApplicationSecurityTokenAndRefreshToken : ApplicationSecurityToken
{
    public string RefreshToken { get; }

    public ApplicationSecurityTokenAndRefreshToken(string accessToken, string refreshToken) : base(accessToken)
    {
        RefreshToken = refreshToken;
    }
}
