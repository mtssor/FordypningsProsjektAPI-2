using Emne9_Fordypningsprosjekt_API.Data;
using Emne9_Fordypningsprosjekt_API.DTOs;
using Emne9_Fordypningsprosjekt_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Emne9_Fordypningsprosjekt_API.Endpoints;

public static class ScoreEndpoints
{
    public static WebApplication MapScoreEndpoints(this WebApplication app)
    {
        var passwordHasher = new PasswordHasher<User>();

        // Update HighScore
        app.MapPost("/api/score", async (
                UpdateHighScore req,
                AppDbContext db,
                HttpContext ctx,
                ILogger<Program> logger) =>
            {
                try
                {
                    var userIdClaim = ctx.User.FindFirst("id")?.Value;
                    if (userIdClaim == null)
                    {
                        logger.LogWarning("Update score attempt without user ID claim");
                        return Results.Unauthorized();
                    }

                    if (!Guid.TryParse(userIdClaim, out var userId))
                    {
                        logger.LogWarning("Invalid GUID in claim: {Claim}", userIdClaim);
                        return Results.Unauthorized();
                    }

                    var user = await db.Users.FindAsync(userId);
                    if (user == null)
                    {
                        logger.LogWarning("Update score failed: user not found ({UserId})", userId);
                        return Results.NotFound();
                    }

                    if (req.NewScore > user.HighScore)
                    {
                        user.HighScore = req.NewScore;
                        await db.SaveChangesAsync();
                        logger.LogInformation("User {UserId} new high score: {Score}", userId, req.NewScore);
                    }
                    else
                    {
                        logger.LogInformation("User {UserId} attempted lower score: {Score}", userId, req.NewScore);
                    }

                    return Results.Ok(new { user.Username, user.HighScore });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error updating high score for user claim {Claim}",
                        ctx.User.FindFirst("id")?.Value);
                    return Results.Problem(
                        title: "Error updating score",
                        detail: ex.Message
                    );
                }
            })
            .RequireAuthorization();
// Get leaderboard
        app.MapGet("/api/leaderboard", async (AppDbContext db, ILogger<Program> logger) =>
        {
            try
            {
                var leaderboard = await db.Users
                    .OrderByDescending(u => u.HighScore)
                    .Select(u => new { u.Username, u.HighScore })
                    .ToListAsync();

                logger.LogInformation("Leaderboard retrieved successfully ({Count} entries)", leaderboard.Count);
                return Results.Ok(leaderboard);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving leaderboard");
                return Results.Problem(
                    title: "Error fetching leaderboard",
                    detail: ex.Message
                );
            }
        }).RequireAuthorization();

        return app;
    }
}