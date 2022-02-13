using Microsoft.IdentityModel.Tokens;

namespace YLunchApi.Authentication.Models;

public class Token
{
    public Token(SecurityToken securityToken, string stringToken)
    {
        SecurityToken = securityToken;
        StringToken = stringToken;
    }

    public SecurityToken SecurityToken { get; }
    public string StringToken { get; }
}
