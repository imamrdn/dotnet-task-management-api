using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.Api.Controllers;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Errors;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _service = new();

    [Fact]
    public async Task GetUsers_ReturnsOk()
    {
        _service.Setup(service => service.GetUsersAsync()).ReturnsAsync([]);

        Assert.IsType<OkObjectResult>(await CreateController().GetUsers());
    }

    [Fact]
    public async Task GetUsersWithoutActiveTasks_ReturnsServiceResponse()
    {
        var response = new List<UserResponse>
        {
            new(3, "Empty", "empty@mail.com")
        };
        _service.Setup(service => service.GetUsersWithoutActiveTasksAsync()).ReturnsAsync(response);

        var result = await CreateController().GetUsersWithoutActiveTasks();

        Assert.Same(response, Assert.IsType<ApiResponse<List<UserResponse>>>(
            Assert.IsType<OkObjectResult>(result).Value).Data);
    }

    [Fact]
    public async Task GetUserById_ReturnsExpectedStatus()
    {
        _service.Setup(service => service.GetUserByIdAsync(1))
            .ReturnsAsync(new UserResponse(1, "User", "user@mail.com"));

        Assert.IsType<NotFoundObjectResult>(await CreateController().GetUserById(0));
        Assert.IsType<OkObjectResult>(await CreateController().GetUserById(1));
        Assert.IsType<NotFoundObjectResult>(await CreateController().GetUserById(2));
    }

    [Fact]
    public async Task GetUserProfile_ReturnsExpectedStatus()
    {
        _service.Setup(service => service.GetUserProfileAsync(1))
            .ReturnsAsync(new UserProfileResponse(1, 1, "Bio", "Location"));

        Assert.IsType<NotFoundObjectResult>(await CreateController().GetUserProfile(0));
        Assert.IsType<OkObjectResult>(await CreateController().GetUserProfile(1));
        Assert.IsType<NotFoundObjectResult>(await CreateController().GetUserProfile(2));
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedOrThrowsForGlobalHandler()
    {
        var request = new CreateUserRequest("User", "user@mail.com", "secret123");
        _service.Setup(service => service.CreateUserAsync(request))
            .ReturnsAsync(new UserResponse(1, request.Name, request.Email));

        var created = Assert.IsType<CreatedResult>(await CreateController().CreateUser(request));
        Assert.True(Assert.IsType<ApiResponse<UserResponse>>(created.Value).Success);

        _service.Setup(service => service.CreateUserAsync(request))
            .ThrowsAsync(new ArgumentException("invalid"));
        await Assert.ThrowsAsync<ArgumentException>(() => CreateController().CreateUser(request));

        _service.Setup(service => service.CreateUserAsync(request))
            .ThrowsAsync(new DuplicateResourceException("duplicate"));
        await Assert.ThrowsAsync<DuplicateResourceException>(() => CreateController().CreateUser(request));
    }

    [Fact]
    public async Task UpsertUserProfile_ReturnsExpectedStatus()
    {
        var request = new UpsertUserProfileRequest("Bio", "Location");
        var controller = CreateController();
        _service.Setup(service => service.UpsertUserProfileAsync(1, request))
            .ReturnsAsync(new UserProfileResponse(1, 1, request.Bio, request.Location));

        Assert.IsType<NotFoundObjectResult>(await controller.UpsertUserProfile(0, request));
        Assert.IsType<OkObjectResult>(await controller.UpsertUserProfile(1, request));

        _service.Setup(service => service.UpsertUserProfileAsync(2, request))
            .ReturnsAsync((UserProfileResponse?)null);
        Assert.IsType<NotFoundObjectResult>(await controller.UpsertUserProfile(2, request));
    }

    [Fact]
    public async Task UpdateUser_ReturnsExpectedStatus()
    {
        var request = new UpdateUserRequest("User", "user@mail.com", null);
        var controller = CreateController();

        Assert.IsType<NotFoundObjectResult>(await controller.UpdateUser(0, request));

        _service.Setup(service => service.UpdateUserAsync(1, request))
            .ReturnsAsync(new UserResponse(1, request.Name, request.Email));
        Assert.IsType<OkObjectResult>(await controller.UpdateUser(1, request));

        _service.Setup(service => service.UpdateUserAsync(2, request)).ReturnsAsync((UserResponse?)null);
        Assert.IsType<NotFoundObjectResult>(await controller.UpdateUser(2, request));

        _service.Setup(service => service.UpdateUserAsync(3, request))
            .ThrowsAsync(new ArgumentException("invalid"));
        await Assert.ThrowsAsync<ArgumentException>(() => controller.UpdateUser(3, request));

        _service.Setup(service => service.UpdateUserAsync(4, request))
            .ThrowsAsync(new DuplicateResourceException("duplicate"));
        await Assert.ThrowsAsync<DuplicateResourceException>(() => controller.UpdateUser(4, request));
    }

    [Fact]
    public async Task DeleteUser_ReturnsExpectedStatus()
    {
        _service.Setup(service => service.DeleteUserAsync(1)).ReturnsAsync(true);
        _service.Setup(service => service.DeleteUserAsync(2)).ReturnsAsync(false);
        var controller = CreateController();

        Assert.IsType<NotFoundObjectResult>(await controller.DeleteUser(0));
        Assert.IsType<NoContentResult>(await controller.DeleteUser(1));
        Assert.IsType<NotFoundObjectResult>(await controller.DeleteUser(2));
    }

    private UsersController CreateController() => new(_service.Object);
}
