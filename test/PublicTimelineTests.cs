using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System;
using System.IO;

namespace Chirp.Razor.test;

public class PublicTimelineTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PublicTimelineTests(TestingWebApplicationFactory factory)
    {
        
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    [Fact]
    public async Task PublicTimeline_ShouldContain_Jacqualine_FirstPage()
    {
        var resp = await _client.GetAsync("/");
        var html = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            File.WriteAllText("PublicTimeline_error.html", html);
        }

        resp.EnsureSuccessStatusCode();
        Assert.Contains("Jacqualine Gilcoine", html);
        Assert.Contains("Starbuck now is what we hear the worst.", html);
    }

    [Fact]
    public async Task MellieTimeline()
    {
        var resp = await _client.GetAsync("/");
        var html = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            File.WriteAllText("MellieTimeline_error.html", html);
        }

        resp.EnsureSuccessStatusCode();
        Assert.Contains("Mellie Yost", html);
        Assert.Contains("But what was behind the barricade.", html);
    }

    [Fact]
    public async Task PublicTimeline_PageParam_1_Equals_Default()
    {
        var r1 = await _client.GetStringAsync("/");
        var r2 = await _client.GetStringAsync("/?page=1");
        Assert.Equal(r1, r2);
    }
}