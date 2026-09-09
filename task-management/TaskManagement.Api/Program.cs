var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var tasks = new List<TaskItem>();


app.MapGet("/api/tasks/{id:int}", (int id) =>
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

app.MapGet("/api/tasks", (int page, int limit) =>
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

app.MapPost("/api/tasks", (CreateTaskRequest request) =>
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

app.MapPut("/api/tasks/{id:int}", (int id, UpdateTaskRequest request) =>
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

app.MapDelete("/api/tasks/{id:int}", (int id) =>
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


class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsCompleted { get; set; }
}
