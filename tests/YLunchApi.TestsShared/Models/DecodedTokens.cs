using YLunchApi.Authentication.Models;

namespace YLunchApi.TestsShared.Models;

public class DecodedTokens : ApplicationSecurityToken
{
    public string RefreshToken { get; }

    public DecodedTokens(string accessToken, string refreshToken) : base(accessToken)
    {
        RefreshToken = refreshToken;
    }
}
