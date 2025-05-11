using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using GameFramework; // Added to resolve the Startup class
using GameFramework.Core; // Added for World and Player
using Microsoft.Extensions.DependencyInjection; // Added for IServiceProvider
using System; // Added for IServiceProvider

// Startup class is in the global namespace, compiled as part of GameFramework.dll
// which is referenced by this SampleGame project.

public class Program // Ensure Program is public for WebApplicationFactory
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        InitializeGame(host.Services);
        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                // Use the Startup class from the GameFramework library
                webBuilder.UseStartup<Startup>(); 
            });

    public static void InitializeGame(IServiceProvider services)
    {
        // Resolve the World service from the dependency injection container
        using (var scope = services.CreateScope())
        {
            var world = scope.ServiceProvider.GetRequiredService<World>();
            // Create a new player and add it to the world
            // Corrected Player constructor call to include id, name, and score
            var player = new Player("player1_id", "Player1", 0); 
            world.AddPlayer(player); // Assuming AddPlayer is the correct method, or AddObject if Player is a WorldObject
        }
    }
}
