using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.UserAggregate;

namespace YLunchApi.Main.Controllers;

public abstract class ApplicationControllerBase : ControllerBase
{
    protected string CurrentUserId => HttpContext.User.FindFirst(x => x.Type == "Id")!.Value;

    protected string CurrentUserEmail => HttpContext.User.Claims.ElementAtOrDefault(1)!.Value;

    protected IEnumerable<string> CurrentUserRoles =>
        Roles.StringToList(HttpContext.User.FindFirst(x => x.Type == ClaimTypes.Role)!.Value); //NOSONAR
}
