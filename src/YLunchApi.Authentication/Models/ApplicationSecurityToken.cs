using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace YLunchApi.Authentication.Models;

public sealed class ApplicationSecurityToken : JwtSecurityToken
{
    public ApplicationSecurityToken(string jwtEncodedString) : base(jwtEncodedString)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(jwtEncodedString);
        AccessToken = jwtEncodedString;
        UserId = jwtSecurityToken.Claims.First(x => x.Type.Equals("Id")).Value;
        UserEmail = jwtSecurityToken.Claims.First(x => x.Type.Equals(JwtRegisteredClaimNames.Sub)).Value;
        var userRolesStr = jwtSecurityToken.Claims.First(x => x.Type.Equals("role")).Value;
        UserRoles = userRolesStr.Split(",").ToList();
    }

    public ApplicationSecurityToken(string jwtEncodedString, string? refreshToken) : this(jwtEncodedString)
    {
        RefreshToken = refreshToken;
    }

    public string AccessToken { get; }
    public string? RefreshToken { get; }
    public string UserId { get; }
    public string UserEmail { get; }
    public List<string> UserRoles { get; }
}
