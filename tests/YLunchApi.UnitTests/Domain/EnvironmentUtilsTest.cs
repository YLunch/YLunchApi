using System;
using FluentAssertions;
using Xunit;
using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.UnitTests.Domain;

public class EnvironmentUtilsTest
{
    [Fact]
    public void Should_Return_OnlineUrl()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        EnvironmentUtils.IsDevelopment.Should().BeFalse();
        EnvironmentUtils.BaseUrl.Should().Be("https://ylunch-api.rael-calitro.ovh");
    }

    [Fact]
    public void Should_Return_LocalUrl()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        EnvironmentUtils.IsDevelopment.Should().BeTrue();
        EnvironmentUtils.BaseUrl.Should().Be("http://localhost:5254");
    }
}
