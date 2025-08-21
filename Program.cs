using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Rewrite;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Middleware to redirect from /tasks to /todos
app.UseRewriter(new RewriteOptions().AddRedirect("tasks/(.*)", "todos/$1"));

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
});

app.MapDelete("/todos/{id}", (int id) =>
    {
    todos.RemoveAll(t=> t.Id == id);
    return TypedResults.NoContent();
});
app.Run();

public record Todo(int Id, string Name, DateTime DueDate, bool IsComplete);
