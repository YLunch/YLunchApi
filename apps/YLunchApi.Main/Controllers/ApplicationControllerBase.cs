using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Authentication.Models;

namespace YLunchApi.Main.Controllers;

public abstract class ApplicationControllerBase : ControllerBase
{
    protected ApplicationControllerBase(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var authorizationHeaderValue = httpContext.Request.Headers.Authorization;
        var authorizationHeaderValueIsToken =
            !new Regex(@"^Bearer [^ ]+\.[^ ]+\.[^ ]+$").IsMatch(authorizationHeaderValue.ToString());
        if (authorizationHeaderValueIsToken) return;

        var accessToken = authorizationHeaderValue.ToString().Replace("Bearer ", "");
        var applicationSecurityToken = new ApplicationSecurityToken(accessToken);

        CurrentAccessTokenId = applicationSecurityToken.Id;
        CurrentUserId = applicationSecurityToken.UserId;
        CurrentUserEmail = applicationSecurityToken.UserEmail;
        CurrentUserRoles = applicationSecurityToken.UserRoles;
    }

    protected string? CurrentAccessTokenId { get; set; }
    protected string? CurrentUserId { get; }
    protected string? CurrentUserEmail { get; }
    protected IEnumerable<string> CurrentUserRoles { get; } = new List<string>();
}
