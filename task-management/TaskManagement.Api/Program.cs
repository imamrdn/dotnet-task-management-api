var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/tasks/{id:int}", (int id) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }

    var task = new
    {
        Id = id,
        Title = $"Task {id}",
        Description = $"This is task {id}",
        IsCompleted = false
    };

    return Results.Ok(task);
});

app.MapGet("/api/tasks", (int page, int limit) =>
{
    if (page <= 0 )
    {
        return Results.BadRequest("Page must be greater than 0");
    }

    if (limit <= 0)
    {
        return Results.BadRequest("Limit must be greater than 0");
    }

    var result = new
    {
        Page = page,
        Limit = limit,
        Message = $"Showing page {page} with limit {limit}"
    };

    return Results.Ok(result);
});

app.MapPost("/api/tasks", (CreateTaskRequest request) =>
{
    var task = new
    {
        Id = 1,
        request.Title,
        request.Description,
        IsCompleted = false
    };

    return Results.Created($"/api/tasks/{task.Id}", task);
});

app.MapDelete("/api/tasks/{id:int}", (int id) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
});

app.Run();

record CreateTaskRequest(string Title, string Description);
