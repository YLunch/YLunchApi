using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using YLunchApi.Authentication.Repositories;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Domain.UserAggregate.Services;

namespace YLunchApi.UnitTests.Core.Configuration;

public class FixtureConfiguration
{
    public string? AccessToken { get; set; }
    public IRefreshTokenRepository? RefreshTokenRepository { get; set; }
    public JwtSecurityTokenHandler? JwtSecurityTokenHandler { get; set; }
    public IUserRepository? UserRepository { get; set; }
    public UserManager<User>? UserManager { get; set; }
    public IDateTimeProvider? DateTimeProvider { get; set; }
    public DateTime? DateTime { get; set; }
}
