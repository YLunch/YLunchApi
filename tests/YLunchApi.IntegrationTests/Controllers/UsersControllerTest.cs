using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Helpers.Extensions;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.TestsShared;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class UsersControllerTest : ControllerTestBase
{
    #region CreateRestaurantAdmin_Tests

    [Theory]
    [InlineData("admin@restaurant.com", "Jean-Marc", "Dupont Henri", "0612345678", "Password1234.")]
    [InlineData("admin.rest@rest-aurant.com", "Jean Marc", "Dupont Henri", "0612345678", "PaSSword1234$")]
    [InlineData("admin-rest@rest.aurant.com", "Jean-Marc", "Dupont-Henri", "0712345678", "paSS@1234word")]
    [InlineData("admin-rest@rest.aurant.com", "Jo", "Do do-do", "0712345678", "paSS@1234word")]
    [InlineData("admin-rest@rest.aurant.com", "John", "Doe", "0712345678", "paSS@1234word")]
    public async Task CreateRestaurantAdmin_Should_Return_A_201Created(string email,
                                                                       string firstname,
                                                                       string lastname,
                                                                       string phoneNumber,
                                                                       string password)
    {
        // Arrange
        var body = new
        {
            Email = email,
            Password = password,
            PhoneNumber = phoneNumber,
            Lastname = lastname,
            Firstname = firstname
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurant-admins", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await ResponseUtils.DeserializeContentAsync<UserReadDto>(response);
        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.Email.Should().Be(body.Email);
        responseBody.PhoneNumber.Should().Be(body.PhoneNumber);
        responseBody.Lastname.Should().Be(body.Lastname.Capitalize());
        responseBody.Firstname.Should().Be(body.Firstname.Capitalize());
    }

    [Fact]
    public async Task CreateRestaurantAdmin_Should_Return_A_409Conflict()
    {
        // Arrange
        var body = new
        {
            UserMocks.RestaurantAdminCreateDto.Email,
            UserMocks.RestaurantAdminCreateDto.Password,
            UserMocks.RestaurantAdminCreateDto.PhoneNumber,
            UserMocks.RestaurantAdminCreateDto.Lastname,
            UserMocks.RestaurantAdminCreateDto.Firstname
        };

        // Act
        _ = await Client.PostAsJsonAsync("restaurant-admins", body);
        var response = await Client.PostAsJsonAsync("restaurant-admins", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should().BeEquivalentTo("User already exists");
    }

    [Fact]
    public async Task CreateRestaurantAdmin_Should_Return_A_400BadRequest_When_Fields_Are_Empty()
    {
        // Arrange
        var body = new { };

        // Act
        var response = await Client.PostAsJsonAsync("restaurant-admins", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should().ContainAll(
            "The Email field is required.",
            "The Password field is required.",
            "The Lastname field is required.",
            "The Firstname field is required.",
            "The PhoneNumber field is required."
        );
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("jean.dupont@")]
    [InlineData("jean.dupont@restaurant")]
    [InlineData("jean.dupont@restaurant.c")]
    [InlineData("Jean.Dupont@Restaurant.com")]
    public async Task CreateRestaurantAdmin_Should_Return_A_400BadRequest_When_Email_Is_Invalid(
        string email
    )
    {
        // Arrange
        var body = new
        {
            Email = email
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurant-admins", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should().Contain("Email is invalid.");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("password1234")]
    [InlineData("password1234.")]
    [InlineData("Pass12.")]
    public async Task CreateRestaurantAdmin_Should_Return_A_400BadRequest_When_Password_Is_Invalid(
        string password
    )
    {
        // Arrange
        var body = new
        {
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurant-admins", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should()
               .Contain(
                   "Password is invalid. Must contain at least 8 characters, 1 lowercase letter, 1 uppercase letter, 1 special character and 1 number.");
    }

    [Theory]
    [InlineData("061234567")]
    [InlineData("0512345678")]
    [InlineData("06123456789")]
    [InlineData("07123456789")]
    public async Task CreateRestaurantAdmin_Should_Return_A_400BadRequest_When_PhoneNumber_Is_Invalid(
        string phoneNumber
    )
    {
        // Arrange
        var body = new
        {
            PhoneNumber = phoneNumber
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurant-admins", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should().Contain("PhoneNumber is invalid. Example: '0612345678'.");
    }

    [Theory]
    [InlineData("jean123", "dupont123")]
    [InlineData("j", "d")]
    [InlineData("Je.", "Du.")]
    [InlineData("Je-A", "Du-P")]
    [InlineData("Je-A", "Du P")]
    public async Task CreateRestaurantAdmin_Should_Return_A_400BadRequest_When_Firstname_Or_Lastname_Is_Invalid(
        string firstname,
        string lastname
    )
    {
        // Arrange
        var body = new
        {
            Firstname = firstname,
            Lastname = lastname
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurant-admins", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should()
               .Contain("Firstname is invalid")
               .And
               .Contain("Lastname is invalid");
    }

    #endregion

    #region CreateCustomer_Tests

    [Theory]
    [InlineData("anne-marie.martin@ynov.com", "Anne-Marie", "Martin-Jacques", "0687654321", "Password1234.")]
    [InlineData("anne.marie@ynov.com", "Anne Marie", "Martin Jacques", "0787654321", "PaSSword1234$")]
    [InlineData("anne.marie@ynov.com", "An-Ma", "Du-He", "0798765432", "paSS@1234word")]
    [InlineData("anne.marie@ynov.com", "An ma", "Du he", "0712345678", "paSS@1234word")]
    [InlineData("anne.marie@ynov.com", "An", "Du", "0712345678", "paSS@1234word")]
    public async Task CreateCustomer_Should_Return_A_201Created(string email,
                                                                string firstname,
                                                                string lastname,
                                                                string phoneNumber,
                                                                string password)
    {
        // Arrange
        var body = new
        {
            Email = email,
            Password = password,
            PhoneNumber = phoneNumber,
            Lastname = lastname,
            Firstname = firstname
        };

        // Act
        var response = await Client.PostAsJsonAsync("customers", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await ResponseUtils.DeserializeContentAsync<UserReadDto>(response);
        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.Email.Should().Be(body.Email);
        responseBody.PhoneNumber.Should().Be(body.PhoneNumber);
        responseBody.Lastname.Should().Be(body.Lastname.Capitalize());
        responseBody.Firstname.Should().Be(body.Firstname.Capitalize());
    }

    [Fact]
    public async Task CreateCustomer_Should_Return_A_409Conflict()
    {
        // Arrange
        var body = new
        {
            UserMocks.CustomerCreateDto.Email,
            UserMocks.CustomerCreateDto.Password,
            UserMocks.CustomerCreateDto.PhoneNumber,
            UserMocks.CustomerCreateDto.Lastname,
            UserMocks.CustomerCreateDto.Firstname
        };

        // Act
        _ = await Client.PostAsJsonAsync("customers", body);
        var response = await Client.PostAsJsonAsync("customers", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should().BeEquivalentTo("User already exists");
    }

    [Fact]
    public async Task CreateCustomer_Should_Return_A_400BadRequest_When_Fields_Are_Empty()
    {
        // Arrange
        var body = new { };

        // Act
        var response = await Client.PostAsJsonAsync("customers", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should().ContainAll("The Email field is required.", "The Password field is required.",
            "The Lastname field is required.", "The Firstname field is required.",
            "The PhoneNumber field is required.");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("jean.dupont@")]
    [InlineData("@ynov.com")]
    [InlineData("jean.dupont@ynov.c")]
    public async Task CreateCustomer_Should_Return_A_400BadRequest_When_Email_Is_Invalid(
        string email
    )
    {
        // Arrange
        var body = new
        {
            Email = email
        };

        // Act
        var response = await Client.PostAsJsonAsync("customers", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should().Contain("Email is invalid. You must provide your Ynov email");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("password1234")]
    [InlineData("password1234.")]
    [InlineData("Pass12.")]
    public async Task CreateCustomer_Should_Return_A_400BadRequest_When_Password_Is_Invalid(
        string password
    )
    {
        // Arrange
        var body = new
        {
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("customers", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should()
               .Contain(
                   "Password is invalid. Must contain at least 8 characters, 1 lowercase letter, 1 uppercase letter, 1 special character and 1 number.");
    }

    [Theory]
    [InlineData("061234567")]
    [InlineData("0512345678")]
    [InlineData("06123456789")]
    [InlineData("07123456789")]
    public async Task CreateCustomer_Should_Return_A_400BadRequest_When_PhoneNumber_Is_Invalid(
        string phoneNumber
    )
    {
        // Arrange
        var body = new
        {
            PhoneNumber = phoneNumber
        };

        // Act
        var response = await Client.PostAsJsonAsync("customers", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should().Contain("PhoneNumber is invalid. Example: '0612345678'.");
    }

    [Theory]
    [InlineData("jean123", "dupont123")]
    [InlineData("j", "d")]
    [InlineData("Je.", "Du.")]
    [InlineData("Je-A", "Du-P")]
    [InlineData("Je-A", "Du P")]
    public async Task CreateCustomer_Should_Return_A_400BadRequest_When_Firstname_Or_Lastname_Is_Invalid(
        string firstname,
        string lastname
    )
    {
        // Arrange
        var body = new
        {
            Firstname = firstname,
            Lastname = lastname
        };

        // Act
        var response = await Client.PostAsJsonAsync("customers", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should()
               .Contain("Firstname is invalid")
               .And
               .Contain("Lastname is invalid");
    }

    #endregion
}
