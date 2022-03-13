using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
using YLunchApi.UnitTests.Core;
using YLunchApi.UnitTests.Core.Mocks;

namespace YLunchApi.UnitTests.Configuration;

public class UnitTestFixtureBase
{
    private ServiceProvider _serviceProvider = null!;
    public string DatabaseId { get; set; } = null!;

    public void InitFixture(Action<FixtureConfiguration>? configureOptions = null)
    {
        var fixtureConfiguration = new FixtureConfiguration();
        configureOptions?.Invoke(fixtureConfiguration);

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddScoped<TrialsController>();
        serviceCollection.AddScoped<UsersController>();
        serviceCollection.AddScoped<RestaurantsController>();
        serviceCollection.AddScoped<AuthenticationController>();


        var context = ContextBuilder.BuildContext(DatabaseId);

        serviceCollection.TryAddScoped<ApplicationDbContext>(_ =>
            context);

        serviceCollection.TryAddScoped<RoleManager<IdentityRole>>(_ =>
            ManagerMocker.GetRoleManagerMock(context).Object);

        serviceCollection.TryAddScoped<UserManager<User>>(_ =>
            ManagerMocker.GetUserManagerMock(context).Object);

        serviceCollection.TryAddScoped<IHttpContextAccessor>(_ =>
            new HttpContextAccessorMock(fixtureConfiguration.AccessToken));

        serviceCollection.TryAddScoped(_ => new JwtSecurityTokenHandler());
        serviceCollection.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        serviceCollection.AddScoped<IUserService, UserService>();

        serviceCollection.AddScoped<IRestaurantRepository, RestaurantRepository>();
        serviceCollection.AddScoped<IRestaurantService, RestaurantService>();


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
        serviceCollection.AddSingleton(tokenValidationParameters);
        serviceCollection.Configure<JwtConfig>(jwtConfig => jwtConfig.Secret = jwtSecret);
        serviceCollection.AddScoped<IJwtService, JwtService>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public T GetImplementationFromService<T>()
    {
        var service = _serviceProvider.GetService<T>();
        return service ?? throw new InvalidOperationException();
    }
}
