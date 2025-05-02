using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Emne9.Fordypningsprosjekt.API.Tests;

public class HealthTests
    : IClassFixture<WebApplicationFactory<Emne9_Fordypningsprosjekt_API.Program>>
{
    private readonly HttpClient _client;

    public HealthTests(
        WebApplicationFactory<Emne9_Fordypningsprosjekt_API.Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress       = new Uri("http://localhost")
        });
    }

    [Fact]
    public async Task Health_Returns_Alive()
    {
        var res = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var body = await res.Content.ReadFromJsonAsync<string>();
        Assert.Equal("alive", body);
    }
}