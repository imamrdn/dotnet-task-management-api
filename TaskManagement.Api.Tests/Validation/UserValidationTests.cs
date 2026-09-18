using TaskManagement.Api.Validation;

namespace TaskManagement.Api.Tests.Validation;

public class UserValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequireName_MissingValue_ThrowsWithExpectedMessage(string? name)
    {
        var exception = Assert.Throws<ArgumentException>(() => UserValidation.RequireName(name));

        Assert.Equal("Name is required", exception.Message);
    }

    [Fact]
    public void RequireName_PresentValue_DoesNotThrow()
    {
        UserValidation.RequireName("User");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequireEmail_MissingValue_ThrowsWithExpectedMessage(string? email)
    {
        var exception = Assert.Throws<ArgumentException>(() => UserValidation.RequireEmail(email));

        Assert.Equal("Email is required", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequirePassword_MissingValue_ThrowsWithExpectedMessage(string? password)
    {
        var exception = Assert.Throws<ArgumentException>(() => UserValidation.RequirePassword(password));

        Assert.Equal("Password is required", exception.Message);
    }

    [Fact]
    public void RequireNameAndEmail_PresentValues_DoNotThrow()
    {
        UserValidation.RequireNameAndEmail("User", "user@mail.com");
    }
}
