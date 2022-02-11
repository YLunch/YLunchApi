using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.UserAggregate;

namespace YLunchApi.Main.Controllers;

public abstract class ApplicationControllerBase : ControllerBase
{
    protected ApplicationControllerBase(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var token = httpContext?.Request.Headers.Authorization;
    }

    protected string CurrentUserId => HttpContext.User.FindFirst(x => x.Type == "Id")!.Value;
    protected string CurrentUserEmail => HttpContext.User.Claims.ElementAtOrDefault(1)!.Value;
    protected IEnumerable<string> CurrentUserRoles =>
        Roles.StringToList(HttpContext.User.FindFirst(x => x.Type == ClaimTypes.Role)!.Value); //NOSONAR
}
