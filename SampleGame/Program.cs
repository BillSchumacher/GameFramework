using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using GameFramework; // Added to resolve the Startup class

// Startup class is in the global namespace, compiled as part of GameFramework.dll
// which is referenced by this SampleGame project.

public class Program // Ensure Program is public for WebApplicationFactory
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                // Use the Startup class from the GameFramework library
                webBuilder.UseStartup<Startup>(); 
            });
}
