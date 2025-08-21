using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Rewrite;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Middleware to redirect from /tasks to /todos
app.UseRewriter(new RewriteOptions().AddRedirect("tasks/(.*)", "todos/$1"));

// Custom middleware to log request details
app.Use(async (context, next) => {
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Started.");
    
    await next(context);
    
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Finished.");
});

var todos = new List<Todo>();
app.MapGet("/todos", () =>todos);
app.MapGet("/", () => "Welcome to the Todo API!");
// GET endpoint
app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) =>
{
    var targetTodo = todos.SingleOrDefault(t => t.Id == id);
    return targetTodo is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(targetTodo);
});

// POST endpoint
app.MapPost("/todos", (Todo task) =>
{
    todos.Add(task);
    return TypedResults.Created($"/todos/{task.Id}", task);
})
.AddEndpointFilter(async (context, next) => {
    var taskArgument = context.GetArgument<Todo>(0);
    var errors = new Dictionary<string, string[]>();
    
    if (taskArgument.DueDate < DateTime.UtcNow)
    {
        errors.Add(nameof(Todo.DueDate), ["Cannot have due date in the past."]);
    }
    
    if (taskArgument.IsComplete)
    {
        errors.Add(nameof(Todo.IsComplete), ["Cannot add completed todo."]);
    }
    
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }
    
    return await next(context);
});

app.MapDelete("/todos/{id}", (int id) =>
    {
    todos.RemoveAll(t=> t.Id == id);
    return TypedResults.NoContent();
});
app.Run();

public record Todo(int Id, string Name, DateTime DueDate, bool IsComplete);
