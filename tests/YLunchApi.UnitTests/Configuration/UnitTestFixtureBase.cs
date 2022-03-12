using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using YLunchApi.Application.RestaurantAggregate;
using YLunchApi.Application.UserAggregate;
using YLunchApi.Authentication.Repositories;
using YLunchApi.Authentication.Services;
using YLunchApi.AuthenticationShared.Repositories;
using YLunchApi.Domain.RestaurantAggregate.Services;
using YLunchApi.Domain.UserAggregate.Services;
using YLunchApi.Infrastructure.Database.Repositories;
using YLunchApi.Main.Controllers;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Configuration;

public class UnitTestFixtureBase
{
    private ServiceProvider _serviceProvider = null!;

    public UnitTestFixtureBase InitFixture(Action<FixtureConfiguration>? configureOptions = null)
    {
        var fixtureConfiguration = new FixtureConfiguration();
        if (configureOptions != null)
        {
            configureOptions(fixtureConfiguration);
        }

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddScoped<TrialsController>();

        serviceCollection.TryAddScoped<IHttpContextAccessor>(_ =>
            new HttpContextAccessorMock(fixtureConfiguration.AccessToken));

        serviceCollection.TryAddScoped(_ => new JwtSecurityTokenHandler());
        serviceCollection.AddScoped<IJwtService, JwtService>();
        serviceCollection.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        serviceCollection.AddScoped<IUserService, UserService>();

        serviceCollection.AddScoped<IRestaurantRepository, RestaurantRepository>();
        serviceCollection.AddScoped<IRestaurantService, RestaurantService>();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        return this;
    }

    public T GetImplementationFromService<T>()
    {
        var service = _serviceProvider.GetService<T>();
        return service ?? throw new InvalidOperationException();
    }
}