using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Errors;

namespace TaskManagement.Api.Tests.Errors;

public class ApiExceptionHandlerTests
{
    [Theory]
    [InlineData(typeof(ArgumentException), "Invalid input", 400, "Invalid input")]
    [InlineData(typeof(UnauthorizedAccessException), "Invalid credentials", 401, "Invalid credentials")]
    [InlineData(typeof(InvalidOperationException), "Email is already registered", 400, "Email is already registered")]
    [InlineData(typeof(InvalidOperationException), "Database unavailable", 500, "An unexpected error occurred")]
    [InlineData(typeof(Exception), "Internal detail", 500, "An unexpected error occurred")]
    public async Task TryHandleAsync_MapsExceptionAndHidesServerDetails(
        Type exceptionType, string exceptionMessage, int expectedStatus, string expectedMessage)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = (Exception)Activator.CreateInstance(exceptionType, exceptionMessage)!;
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        context.Response.Body.Position = 0;
        var response = await JsonSerializer.DeserializeAsync<ApiResponse<object>>(
            context.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.True(handled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.False(response!.Success);
        Assert.Equal(expectedMessage, response.Message);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task TryHandleAsync_RequestAbortedCancellation_ReturnsClientClosedRequestWithoutErrorBody()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        using var requestAbortedSource = new CancellationTokenSource();
        await requestAbortedSource.CancelAsync();
        context.RequestAborted = requestAbortedSource.Token;
        var logger = new TestLogger<ApiExceptionHandler>();
        var handler = new ApiExceptionHandler(logger);

        var handled = await handler.TryHandleAsync(
            context,
            new OperationCanceledException(requestAbortedSource.Token),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(499, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
        Assert.DoesNotContain(logger.Entries, entry => entry.Level == LogLevel.Error);
    }
}
