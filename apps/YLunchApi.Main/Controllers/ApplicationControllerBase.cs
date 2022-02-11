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
        if (authorizationHeaderValue.ToString() == "") return;

        var accessToken = authorizationHeaderValue.ToString().Replace("Bearer ", "");
        var applicationSecurityToken = new ApplicationSecurityToken(accessToken);
        CurrentUserId = applicationSecurityToken.UserId;
        CurrentUserEmail = applicationSecurityToken.UserEmail;
        CurrentUserRoles = applicationSecurityToken.UserRoles;
    }

    protected string? CurrentUserId { get; }
    protected string? CurrentUserEmail { get; }
    protected IEnumerable<string> CurrentUserRoles { get; } = new List<string>();
}
