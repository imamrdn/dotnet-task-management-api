using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
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

var taskEndpoints = app.MapGroup("/api/tasks");

taskEndpoints.MapGet("", async (int page, int limit, AppDbContext db) =>
{
    if (page <= 0)
    {
        return Results.BadRequest("Page must be greater than 0");
    }

    if (limit <= 0)
    {
        return Results.BadRequest("Limit must be greater than 0");
    }

    var taskItems = await db.Tasks
        .Select(task => new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted
        ))
        .ToListAsync();

    return Results.Ok(taskItems);
});

taskEndpoints.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }

    var task = await db.Tasks.FirstOrDefaultAsync(task => task.Id == id);

    if (task is null)
    {
        return Results.NotFound();
    }

    var response = new TaskResponse(
        task.Id,
        task.Title,
        task.Description,
        task.IsCompleted
    );

    return Results.Ok(response);
});

taskEndpoints.MapPost("", async (CreateTaskRequest request, AppDbContext db) =>
{
    var task = new TaskItem
    {
        Title = request.Title,
        Description = request.Description,
        IsCompleted = false
    };

    db.Tasks.Add(task);
    await db.SaveChangesAsync();

    var response = new TaskResponse(
        task.Id,
        task.Title,
        task.Description,
        task.IsCompleted
    );

    return Results.Created($"/api/tasks/{task.Id}", response);
});

taskEndpoints.MapPut("/{id:int}", async (int id, UpdateTaskRequest request, AppDbContext db) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }

    var task = await db.Tasks.FirstOrDefaultAsync(task => task.Id == id);

    if (task is null)
    {
        return Results.NotFound();
    }

    task.Title = request.Title;
    task.Description = request.Description;
    task.IsCompleted = request.IsCompleted;

    await db.SaveChangesAsync();

    var response = new TaskResponse(
        task.Id,
        task.Title,
        task.Description,
        task.IsCompleted
    );

    return Results.Ok(response);
});

taskEndpoints.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }

    var task = await db.Tasks.FirstOrDefaultAsync(task => task.Id == id);

    if (task is null)
    {
        return Results.NotFound();
    }

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
