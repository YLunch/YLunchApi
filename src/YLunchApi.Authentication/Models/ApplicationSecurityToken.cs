using System.IdentityModel.Tokens.Jwt;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Authentication.Models;

public class ApplicationSecurityToken : JwtSecurityToken
{
    public string AccessToken { get; }
    public string UserId { get; }
    public string UserEmail { get; }
    public List<string> UserRoles { get; }

    public ApplicationSecurityToken(string jwtEncodedString) : base(jwtEncodedString)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(jwtEncodedString);
        AccessToken = jwtEncodedString;
        UserId = jwtSecurityToken.Claims.First(x => x.Type.Equals("Id")).Value;
        UserEmail = jwtSecurityToken.Claims.First(x => x.Type.Equals(JwtRegisteredClaimNames.Sub)).Value;
        var roles = jwtSecurityToken.Claims.First(x => x.Type.Equals("role")).Value;
        UserRoles = Roles.StringToList(roles);
    }
}
