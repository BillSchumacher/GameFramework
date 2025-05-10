using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
// Program class is expected to be in the global namespace from the referenced SampleGame project

namespace GameFramework.Tests
{
    public class WebServerTests : IClassFixture<WebApplicationFactory<Program>> // Changed Startup to Program
    {
        private readonly HttpClient _client;

        public WebServerTests(WebApplicationFactory<Program> factory) // Changed Startup to Program
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetGameEndpoint_ShouldReturnDefaultGame()
        {
            // Act
            var response = await _client.GetAsync("/game");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Web Game from GameFramework Startup", content); // Updated assertion
        }
    }
}
