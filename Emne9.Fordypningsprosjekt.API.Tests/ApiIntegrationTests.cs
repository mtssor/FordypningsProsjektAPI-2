using System;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Emne9_Fordypningsprosjekt_API;
using Emne9_Fordypningsprosjekt_API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;
using LoginRequest = Emne9_Fordypningsprosjekt_API.DTOs.LoginRequest;
using RegisterRequest = Emne9_Fordypningsprosjekt_API.DTOs.RegisterRequest;

namespace Emne9.Fordypningsprosjekt.API.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Emne9_Fordypningsprosjekt_API.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)

    {
        builder.UseEnvironment("Testing");
    }
}

// test Register + Login
public class AuthTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("http://localhost")
        });
    }

    [Fact(DisplayName = "Register → 201 Created")]
    public async Task Register_Returns_Created()
    {
        var req = new RegisterRequest(
            "u" + Guid.NewGuid().ToString("N").Substring(0, 6),
            $"{Guid.NewGuid():N}@test.local",
            "P@ssw0rd!"
        );

        var resp = await _client.PostAsJsonAsync("/api/auth/register", req);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact(DisplayName = "Login invalid creds → 401 Unauthorized")]
    public async Task Login_InvalidCreds_Returns_401()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("no-user", "wrongpass"));
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact(DisplayName = "Register + Login valid creds → token")]
    public async Task Login_ValidCreds_Returns_Token()
    {
        // 1) Register
        var username = "v" + Guid.NewGuid().ToString("N").Substring(0,6);
        var email    = $"{username}@test.local";
        var password = "Secret123!";
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(username, email, password));

        // 2) Login
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(username, password));
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        // 3) Extract token
        var payload = await loginResp.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrEmpty(payload?.Token));
    }

    // helper for login JSON
    private class TokenResponse
    {
        public string Token { get; set; } = default!;
    }
}