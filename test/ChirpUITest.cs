using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Chirp.test;

public class ChirpUITest : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChirpUITest(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LogInBeforePost()
    {
        // Arrange + Act
        var response = await _client.GetAsync("/postCheep");
        var html = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("YOU HAVE TO LOG IN TO USE THIS FUNCTION", html);
    }
}