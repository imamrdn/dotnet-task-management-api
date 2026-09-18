using TaskManagement.Api.DTOs;
using TaskManagement.Api.Validation;

namespace TaskManagement.Api.Tests.Validation;

public class RequestValidationTests
{
    [Theory]
    [InlineData("", "user@mail.com", "secret123", "Name is required")]
    [InlineData("User", "", "secret123", "Email is required")]
    [InlineData("User", "user@mail.com", "", "Password is required")]
    public void EnsureValid_RegisterRequest_MissingField_ThrowsWithExpectedMessage(
        string name, string email, string password, string expectedMessage)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            RequestValidation.EnsureValid(new RegisterRequest(name, email, password)));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("", "secret123", "Email is required")]
    [InlineData("user@mail.com", "", "Password is required")]
    public void EnsureValid_LoginRequest_MissingField_ThrowsWithExpectedMessage(
        string email, string password, string expectedMessage)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            RequestValidation.EnsureValid(new LoginRequest(email, password)));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void EnsureValid_CreateUserRequest_MissingName_ThrowsWithExpectedMessage()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            RequestValidation.EnsureValid(new CreateUserRequest("", "user@mail.com", "secret123")));

        Assert.Equal("Name is required", exception.Message);
    }

    [Fact]
    public void EnsureValid_UpdateUserRequest_MissingName_ThrowsWithExpectedMessage()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            RequestValidation.EnsureValid(new UpdateUserRequest("", "user@mail.com", null)));

        Assert.Equal("Name is required", exception.Message);
    }

    [Fact]
    public void EnsureValid_ValidRequest_DoesNotThrow()
    {
        RequestValidation.EnsureValid(new RegisterRequest("User", "user@mail.com", "secret123"));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("user@")]
    public void EnsureValid_InvalidEmailFormat_ThrowsWithDtoMessage(string email)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            RequestValidation.EnsureValid(new RegisterRequest("User", email, "secret123")));

        Assert.Equal("Email is invalid", exception.Message);
    }
}
