using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Authentication.Services;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Services;

namespace YLunchApi.Main.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ApplicationControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;

    public AuthenticationController(IJwtService jwtService,
                                    IUserService userService,
                                    IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _jwtService = jwtService;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenReadDto>> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        try
        {
            var authenticatedUser = await _userService.GetAuthenticatedUser(loginRequestDto);
            return Ok(await _jwtService.GenerateJwtToken(authenticatedUser));
        }
        catch
        {
            return Unauthorized(new ErrorDto(HttpStatusCode.Unauthorized, "Please login with valid credentials."));
        }
    }

    [HttpPost("refresh-tokens")]
    public async Task<ActionResult<TokenReadDto>> RefreshTokens([FromBody] TokenUpdateDto tokenUpdateDto)
    {
        try
        {
            var tokenReadDto = await _jwtService.RefreshJwtToken(tokenUpdateDto);
            return Ok(tokenReadDto);
        }
        catch
        {
            if (EnvironmentUtils.IsDevelopment) throw;

            return Unauthorized(new ErrorDto(HttpStatusCode.Unauthorized,
                "Invalid tokens, please login to generate new valid tokens."));
        }
    }

    [HttpGet("current-user")]
    [Authorize]
    public async Task<ActionResult<UserReadDto>> GetCurrentUser()
    {
        try
        {
            return Ok(await _userService.GetUserById(CurrentUserId!));
        }
        catch (EntityNotFoundException)
        {
            return Unauthorized(new ErrorDto(HttpStatusCode.Unauthorized,
                "Invalid tokens, please login to generate new valid tokens."));
        }
    }

    [HttpGet("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        await _jwtService.RevokeRefreshToken(CurrentAccessTokenId!);
        return NoContent();
    }
}
