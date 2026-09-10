using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var tasks = new List<TaskItem>();

var taskEndpoints = app.MapGroup("/api/tasks");

taskEndpoints.MapGet("", (int page, int limit) =>
{
    if (page <= 0)
    {
        return Results.BadRequest("Page must be greater than 0");
    }

    if (limit <= 0)
    {
        return Results.BadRequest("Limit must be greater than 0");
    }

    return Results.Ok(tasks);
});

taskEndpoints.MapGet("/{id:int}", (int id) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }

    var task = tasks.FirstOrDefault(task => task.Id == id);

    if (task is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(task);
});

taskEndpoints.MapPost("", (CreateTaskRequest request) =>
{
    var task = new TaskItem
    {
        Id = tasks.Count + 1,
        Title = request.Title,
        Description = request.Description,
        IsCompleted = false
    };

    tasks.Add(task);

    return Results.Created($"/api/tasks/{task.Id}", task);
});

taskEndpoints.MapPut("/{id:int}", (int id, UpdateTaskRequest request) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }

    var task = tasks.FirstOrDefault(task => task.Id == id);

    if (task is null)
    {
        return Results.NotFound();
    }

    task.Title = request.Title;
    task.Description = request.Description;
    task.IsCompleted = request.IsCompleted;

    return Results.Ok(task);
});

taskEndpoints.MapDelete("/{id:int}", (int id) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }

    var task = tasks.FirstOrDefault(task => task.Id == id);

    if (task is null)
    {
        return Results.NotFound();
    }

    tasks.Remove(task);

    return Results.NoContent();
});

app.Run();

record CreateTaskRequest(string Title, string Description);
record UpdateTaskRequest(string Title, string Description, bool IsCompleted);
