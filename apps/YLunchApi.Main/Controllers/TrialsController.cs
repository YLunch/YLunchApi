using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Main.Controllers;

[ApiController]
[Route("[controller]")]
public class TrialsController : ApplicationControllerBase
{
    public TrialsController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    [HttpGet("anonymous")]
    public ActionResult<string> GetAnonymousTry()
    {
        return Ok(new MessageDto("YLunchApi is running, you are anonymous."));
    }

    [HttpGet("authenticated")]
    [Authorize]
    public ActionResult<string> GetAuthenticatedTry()
    {
        return Ok(new MessageDto(
            $"YLunchApi is running, you are authenticated as {CurrentUserEmail} with Id: {CurrentUserId} and Roles: {Roles.ListToString(CurrentUserRoles)}."));
    }

    [HttpGet("authenticated-customer")]
    [Authorize(Roles = Roles.Customer)]
    public ActionResult<string> GetAuthenticatedCustomerTry()
    {
        return Ok(new MessageDto(
            $"YLunchApi is running, you are authenticated as {CurrentUserEmail} with Id: {CurrentUserId} and Roles: {Roles.ListToString(CurrentUserRoles)}."));
    }

    [HttpGet("authenticated-restaurant-admin")]
    [Authorize(Roles = Roles.RestaurantAdmin)]
    public ActionResult<string> GetAuthenticatedRestaurantAdminTry()
    {
        return Ok(new MessageDto(
            $"YLunchApi is running, you are authenticated as {CurrentUserEmail} with Id: {CurrentUserId} and Roles: {Roles.ListToString(CurrentUserRoles)}."));
    }
}
