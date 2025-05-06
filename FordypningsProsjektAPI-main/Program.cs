using System.Numerics;
using Emne9_Fordypningsprosjekt_API.Endpoints;
using Emne9_Fordypningsprosjekt_API.Models;
using Emne9_Fordypningsprosjekt_API.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/api-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7
        )
    .CreateLogger();

builder.Host.UseSerilog();

// 1) Registers services
builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseRateLimiter();

// ───────────────────────────────────────────────────────────
// 2) Error pages + HTTPS/HSTS
//    In Development → show detailed errors + Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();          // ←– show full stack trace in browser
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // 2a) In Production/Testing → generic error JSON
    app.UseExceptionHandler(errApp =>
    {
        errApp.Run(async context =>
        {
            var logger = context.RequestServices
                                .GetRequiredService<ILogger<Program>>();
            var ex = context.Features
                           .Get<IExceptionHandlerFeature>()?.Error;

            logger.LogError(ex, "Unhandled exception");

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                title  = "An unexpected error occurred.",
                status = context.Response.StatusCode,
                detail = (string?)null     // ←– never leak ex.Message here
            });
        });
    });

    // 2b) Force HTTPS redirection + send HSTS header
    app.UseHttpsRedirection();  // ←– redirect http:// → https://
    app.UseHsts();              // ←– add Strict-Transport-Security header
}

// ───────────────────────────────────────────────────────────
// 3) Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// ───────────────────────────────────────────────────────────
// 4) Your endpoint registrations
//    (plus your PasswordHasher helper if you still need it)
var passwordHasher = new PasswordHasher<User>();

app
    .MapAuthEndpoints()
    .MapScoreEndpoints();

// ───────────────────────────────────────────────────────────
// 5) Health check
app.MapGet("/health", () => Results.Ok("alive"));

app.Run();