using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using YLunchApi.Application.RestaurantAggregate;
using YLunchApi.Application.UserAggregate;
using YLunchApi.Authentication.Models;
using YLunchApi.Authentication.Repositories;
using YLunchApi.Authentication.Services;
using YLunchApi.AuthenticationShared.Repositories;
using YLunchApi.Domain.RestaurantAggregate.Services;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Domain.UserAggregate.Services;
using YLunchApi.Infrastructure.Database;
using YLunchApi.Infrastructure.Database.Repositories;
using YLunchApi.Main.Controllers;
using YLunchApi.UnitTests.Core.Mocks;

namespace YLunchApi.UnitTests.Core.Configuration;

public class UnitTestFixtureBase
{
    private ServiceProvider _serviceProvider = null!;
    private ServiceCollection _serviceCollection = null!;
    public string DatabaseId { get; set; } = null!;

    private void PrepareDependencies(FixtureConfiguration fixtureConfiguration)
    {
        _serviceCollection = new ServiceCollection();

        _serviceCollection.AddScoped<TrialsController>();
        _serviceCollection.AddScoped<UsersController>();
        _serviceCollection.AddScoped<RestaurantsController>();
        _serviceCollection.AddScoped<AuthenticationController>();

        _serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase($"YLunchDatabaseForUnitTests-{DatabaseId}"));

        _serviceCollection.AddScoped<RoleManager<IdentityRole>, RoleManagerMock>();

        _serviceCollection.AddScoped<UserManager<User>, UserManagerMock>();

        _serviceCollection.TryAddScoped<IHttpContextAccessor>(_ =>
            new HttpContextAccessorMock(fixtureConfiguration.AccessToken));

        _serviceCollection.TryAddScoped(_ => new JwtSecurityTokenHandler());
        _serviceCollection.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        _serviceCollection.AddScoped<IUserRepository, UserRepository>();
        _serviceCollection.AddScoped<IUserService, UserService>();

        _serviceCollection.AddScoped<IRestaurantRepository, RestaurantRepository>();
        _serviceCollection.AddScoped<IRestaurantService, RestaurantService>();

        // For Jwt
        const string jwtSecret = "JsonWebTokenSecretForTests";
        var optionsMonitorMock = Substitute.For<IOptionsMonitor<JwtConfig>>();
        optionsMonitorMock.CurrentValue.Returns(new JwtConfig
        {
            Secret = jwtSecret
        });

        var key = Encoding.ASCII.GetBytes(jwtSecret);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            RequireExpirationTime = false,

            // Allow to use seconds for expiration of token
            // Required only when token lifetime less than 5 minutes
            ClockSkew = TimeSpan.Zero
        };
        _serviceCollection.AddSingleton(tokenValidationParameters);
        _serviceCollection.Configure<JwtConfig>(jwtConfig => jwtConfig.Secret = jwtSecret);
        _serviceCollection.AddScoped<IJwtService, JwtService>();

        _serviceProvider = _serviceCollection.BuildServiceProvider();
    }

    public void InitFixture(Action<FixtureConfiguration>? configureOptions = null)
    {
        var fixtureConfiguration = new FixtureConfiguration();
        configureOptions?.Invoke(fixtureConfiguration);

        PrepareDependencies(fixtureConfiguration);
        ReplaceDependencies(fixtureConfiguration);
    }

    private void ReplaceDependencies(FixtureConfiguration fixtureConfiguration)
    {
        if (fixtureConfiguration.RefreshTokenRepository != null)
        {
            _serviceCollection.Remove(
                _serviceCollection.First(descriptor => descriptor.ServiceType == typeof(IRefreshTokenRepository)));
            _serviceCollection.TryAddScoped<IRefreshTokenRepository>(_ => fixtureConfiguration.RefreshTokenRepository!);
        }

        if (fixtureConfiguration.JwtSecurityTokenHandler != null)
        {
            _serviceCollection.Remove(
                _serviceCollection.First(descriptor => descriptor.ServiceType == typeof(JwtSecurityTokenHandler)));
            _serviceCollection.TryAddScoped<JwtSecurityTokenHandler>(_ =>
                fixtureConfiguration.JwtSecurityTokenHandler!);
        }

        if (fixtureConfiguration.UserRepository != null)
        {
            _serviceCollection.Remove(
                _serviceCollection.First(descriptor => descriptor.ServiceType == typeof(IUserRepository)));
            _serviceCollection.TryAddScoped<IUserRepository>(_ => fixtureConfiguration.UserRepository!);
        }

        if (fixtureConfiguration.UserManager != null)
        {
            _serviceCollection.Remove(
                _serviceCollection.First(descriptor => descriptor.ServiceType == typeof(UserManager<User>)));
            _serviceCollection.TryAddScoped<UserManager<User>>(_ => fixtureConfiguration.UserManager!);
        }

        _serviceProvider = _serviceCollection.BuildServiceProvider();
    }

    public T GetImplementationFromService<T>()
    {
        var service = _serviceProvider.GetService<T>();
        return service ?? throw new InvalidOperationException();
    }
}
