using Emne9_Fordypningsprosjekt_API.Endpoints;
using Emne9_Fordypningsprosjekt_API.Models;
using Emne9_Fordypningsprosjekt_API.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Global Exception Handler
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        logger.LogError(ex, "Unhandled exception");

        context.Response.ContentType = "application/problems+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            title = "An unexpected error occured.",
            status = context.Response.StatusCode,
            detail = app.Environment.IsDevelopment() ? ex?.Message : null
        });
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Helper: Password hasher for User entities
var passwordHasher = new PasswordHasher<User>();

// API Endpoints
app
    .MapAuthEndpoints()
    .MapScoreEndpoints();

// Canary
app.MapGet("/health", () => Results.Ok("alive"));

app.Run();