using System;
using System.Text;
using System.Threading.RateLimiting;
using Emne9_Fordypningsprosjekt_API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Emne9_Fordypningsprosjekt_API.Services;

public static class ServiceExtensions
{
    /// <summary>
    /// Registers EF Core, JWT auth, authorization, custom services, and Swagger.
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
    {
        // EF Core + MySQL
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // JWT Authentication
        var jwtSection = config.GetSection("Jwt");
        var keyBytes   = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                    };
                });
        
        // Rate Limiting
        services.AddRateLimiter(opts =>
        {
            opts.RejectionStatusCode = 429; // too many requests

            opts.AddPolicy("loginPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,     // max attempts 5.
                        Window = TimeSpan.FromMinutes(15),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });

        // Authorization
        services.AddAuthorization();

        // JwtService
        services.AddScoped<JwtService>();

        // Swagger / OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name        = "Authorization",
                In          = ParameterLocation.Header,
                Type        = SecuritySchemeType.ApiKey
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        },
                        In     = ParameterLocation.Header,
                        Name   = "Authorization",
                        Scheme = "Bearer"
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}