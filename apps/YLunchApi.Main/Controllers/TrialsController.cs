using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.UserAggregate;

namespace YLunchApi.Main.Controllers;

[ApiController]
[Route("[controller]")]
public class TrialsController : ApplicationControllerBase
{
    [HttpGet("anonymous")]
    public ActionResult<string> GetAnonymousTry()
    {
        return Ok("YLunchApi is running, you are anonymous");
    }

    [HttpGet("authenticated")]
    [Authorize]
    public ActionResult<string> GetAuthenticatedTry()
    {
        return Ok($"YLunchApi is running, you are authenticated as {CurrentUserEmail} with Id: {CurrentUserId} and Roles: {string.Join(";", CurrentUserRoles)}");
    }
}
