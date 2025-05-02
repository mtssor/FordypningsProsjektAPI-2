using Emne9_Fordypningsprosjekt_API.Data;
using Emne9_Fordypningsprosjekt_API.Models;
using Emne9_Fordypningsprosjekt_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Emne9_Fordypningsprosjekt_API.DTOs;

namespace Emne9_Fordypningsprosjekt_API.Endpoints;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var passwordHasher = new PasswordHasher<User>();

        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");
// API endpoints
// Registration
        group.MapPost("/register", async (
            RegisterRequest registerRequest,
            AppDbContext db,
            ILogger<Program> logger) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(registerRequest.Username) ||
                    string.IsNullOrWhiteSpace(registerRequest.Email) ||
                    string.IsNullOrWhiteSpace(registerRequest.Password))
                {
                    return Results.BadRequest(new { message = "All fields are required." });
                }

                if (await db.Users.AnyAsync(u =>
                        u.Username == registerRequest.Username || u.Email == registerRequest.Email))
                {
                    return Results.BadRequest(new { message = "User with provided username or email already exists." });
                }

                var newUser = new User
                {
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    HighScore = 0 // initial highscore
                };

                newUser.PasswordHash = passwordHasher.HashPassword(newUser, registerRequest.Password);

                db.Users.Add(newUser);
                await db.SaveChangesAsync();

                return Results.Created($"/register/{newUser.Username}",
                    new { newUser.Id, newUser.Username, newUser.Email });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in registration for {Username}", registerRequest.Username);
                return Results.Problem(
                    title: "Registration failed",
                    detail: ex.Message
                );
            }
        });

// login
        group.MapPost("/login", async (
            LoginRequest loginRequest,
            AppDbContext db,
            JwtService jwtService,
            ILogger<Program> logger) =>
        {
            try
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);
                if (user == null)
                {
                    logger.LogWarning("Login failed: user not fount ({Username})", loginRequest.Username);
                    return Results.Unauthorized();
                }

                var verificationResult =
                    passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    logger.LogWarning("Login failed: invalid password for user {Username}", loginRequest.Username);
                    return Results.Unauthorized();
                }

                var token = jwtService.GenerateToken(user);
                logger.LogInformation("User logged in: {Username}", loginRequest.Username);
                return Results.Ok(new { token });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in login for {Username}", loginRequest.Username);
                return Results.Problem(
                    title: "Login failed",
                    detail: ex.Message
                );
            }
        });
        return app;
    }
}