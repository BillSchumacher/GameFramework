using Xunit;
using SampleGame; // Assuming Program.cs is in the SampleGame namespace
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Moq;

namespace GameFramework.Tests.SampleGame
{
    public class ProgramTests
    {
        [Fact]
        public void Main_Should_BuildAndRunHost()
        {
            // Arrange
            var hostBuilderMock = new Mock<IHostBuilder>();
            var hostMock = new Mock<IHost>();

            hostBuilderMock.Setup(b => b.Build()).Returns(hostMock.Object);

            // Act
            // To test Main, we need a way to inject/mock CreateHostBuilder or verify its effects.
            // For simplicity here, we'll assume Main calls CreateHostBuilder and then Build().Run().
            // A more involved test might require refactoring Program.Main for better testability
            // or using a test server if it's an ASP.NET Core app.

            // This is a simplified way to check if Main executes without throwing.
            // A full integration test for Main is more complex.
            var ex = Record.Exception(() => Program.Main(new string[] { }));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void CreateHostBuilder_Should_ConfigureWebHost()
        {
            // Arrange
            string[] args = new string[] { };

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);
            var host = hostBuilder.Build(); // Build to ensure configuration is valid

            // Assert
            Assert.NotNull(host);
            // Further assertions can be made here if specific services or configurations
            // are expected to be set up by CreateHostBuilder.
        }
    }
}
