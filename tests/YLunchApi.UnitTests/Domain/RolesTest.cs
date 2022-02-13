using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.UnitTests.Domain;

public class RolesTest
{
    [Fact]
    public void Should_Return_All_The_Application_Roles()
    {
        Roles.GetList().Should().BeEquivalentTo(new List<string>
        {
            Roles.Customer,
            Roles.RestaurantAdmin,
            Roles.SuperAdmin
        });
    }
}
