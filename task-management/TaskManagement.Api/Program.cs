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
    return new
    {
        Id = id,
        Title = $"Task {id}",
        Description = $"This is task {id}",
        IsCompleted = false
    };
});

app.MapGet("/api/tasks", (int page, int limit) =>
{
    return new
    {
        Page = page,
        Limit = limit,
        Message = $"Showing page {page} with limit {limit}"
    };
});

app.MapPost("/api/tasks", (CreateTaskRequest request) =>
{
    return new
    {
        Id = 1,
        request.Title,
        request.Description,
        IsCompleted = false
    };
});

app.Run();

record CreateTaskRequest(string Title, string Description);
