using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GameFramework.Core;
using System.Linq; // Added for LINQ queries
using System.Text.Json; // Added for JsonSerializer

namespace GameFramework
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services required by your GameFramework for web functionalities
            services.AddRouting(); 
            services.AddSingleton<World>(); // Register World as a singleton service
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Serve static files from wwwroot
            app.UseDefaultFiles(); // Enables default file mapping (e.g., index.html)
            app.UseStaticFiles(); // Serves files from wwwroot

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/game", async context =>
                {
                    var world = context.RequestServices.GetRequiredService<World>();
                    var gameData = new 
                    {
                        playerCount = world.Players.Count,
                        worldObjectsCount = world.Objects.Count, // Changed from objectCount
                        firstObjectName = world.Objects.FirstOrDefault()?.Name ?? "N/A"
                    };
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(gameData));
                });
                // Define other endpoints your GameFramework might expose
            });
        }
    }
}
