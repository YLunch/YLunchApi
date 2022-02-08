using Microsoft.AspNetCore.Mvc;

namespace YLunchApi.Main.Controllers;

public abstract class ApplicationControllerBase : ControllerBase
{
    protected string CurrentUserId => HttpContext.User.FindFirst(x => x.Type == "Id")!.Value;

    protected string CurrentUserEmail => HttpContext.User.Claims.ElementAtOrDefault(1)!.Value;

    protected IEnumerable<string> CurrentUserRoles =>
        HttpContext.User.FindFirst(x => x.Type == "Roles")!.Value.Split(";").ToList(); //NOSONAR
}
