using UserManagmentAPI.Models;
using UserManagmentAPI.Services;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<UserRepository>();

var app = builder.Build();
var logger = app.Logger;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Error-handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        // Log the exception (to console or logger)
        Console.WriteLine($"Unhandled exception: {ex.Message}");

        // Set response details
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var errorResponse = new { error = "Internal server error." };

        // Write JSON response
        await context.Response.WriteAsJsonAsync(errorResponse);
    }
});

// Token validation middleware
app.Use(async (context, next) =>
{
    // Check if the Authorization header exists
    if (!context.Request.Headers.TryGetValue("Authorization", out var token))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "Missing Authorization header." });
        return;
    }

    // Simple validation: check if token matches a known value
    // In real apps, you'd validate JWTs or use IdentityServer
    var expectedToken = "my-secret-token"; // replace with your real token or JWT validation
    if (token != $"Bearer {expectedToken}")
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid token." });
        return;
    }

    // If valid, continue to next middleware
    await next.Invoke();
});

// Logging middleware
app.Use(async (context, next) =>
{
    // Log request method and path
    Console.WriteLine($"Incoming Request: {context.Request.Method} {context.Request.Path}");
    logger.LogInformation("Incoming Request: {method} {path}", context.Request.Method, context.Request.Path);

    // Call the next middleware in the pipeline
    await next.Invoke();

    // Log response status code
    Console.WriteLine($"Response Status: {context.Response.StatusCode}");
    logger.LogInformation("Response Status: {statusCode}", context.Response.StatusCode);
});


// 🧑‍💼 User Management Endpoints
var repo = app.Services.GetRequiredService<UserRepository>();

app.MapGet("/users", (int? page, int? pageSize) =>
{
    const int defaultPageSize = 20;
    var skip = ((page ?? 1) - 1) * (pageSize ?? defaultPageSize);
    var take = pageSize ?? defaultPageSize;

    var users = repo.GetAll()
                    .Skip(skip)
                    .Take(take);

    return Results.Ok(users);
});

app.MapGet("/users/{id}", (int id) =>
{
    var user = repo.GetById(id);
    return user is not null 
        ? Results.Ok(user) 
        : Results.NotFound(new { message = $"User with ID {id} not found" });
});

app.MapPost("/users", (User user) =>
{
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(user);
    if (!Validator.TryValidateObject(user, context, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    var created = repo.Create(user);
    return Results.Created($"/users/{created.Id}", created);
});

app.MapPut("/users/{id}", (int id, User updatedUser) =>
{
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(updatedUser);
    if (!Validator.TryValidateObject(updatedUser, context, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    return repo.Update(id, updatedUser) ? Results.NoContent() : Results.NotFound(new { message = $"User with ID {id} not found" });
});

app.MapDelete("/users/{id}", (int id) =>
{
    return repo.Delete(id) 
        ? Results.NoContent() 
        : Results.NotFound(new { message = $"User with ID {id} not found" });
});

app.Run();
