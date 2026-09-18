using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Extensions;

public static class ControllerExtensions
{
    public static bool IsValidId(this int id) => id > 0;

    /// <summary>
    /// Returns 200 OK when <paramref name="data"/> is not null,
    /// otherwise 404 Not Found. Use for single-resource endpoints.
    /// </summary>
    public static IActionResult Reply<T>(
        this ControllerBase controller,
        T? data,
        string okMessage,
        string notFoundMessage) =>
        data is null
            ? controller.NotFound(ApiResponse<object>.Error(notFoundMessage))
            : controller.Ok(ApiResponse<T>.Ok(okMessage, data));

    /// <summary>
    /// Returns 204 No Content when <paramref name="deleted"/> is true,
    /// otherwise 404 Not Found. Use for delete endpoints.
    /// </summary>
    public static IActionResult Reply(
        this ControllerBase controller,
        bool deleted,
        string notFoundMessage) =>
        deleted
            ? controller.NoContent()
            : controller.NotFound(ApiResponse<object>.Error(notFoundMessage));

    /// <summary>
    /// Always returns 200 OK. Use for endpoints whose data is never null,
    /// for example list endpoints that return an empty list instead of null.
    /// </summary>
    public static IActionResult Reply<T>(
        this ControllerBase controller,
        T data,
        string okMessage) =>
        controller.Ok(ApiResponse<T>.Ok(okMessage, data));
}
