using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameFramework
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services required by your GameFramework for web functionalities
            services.AddRouting(); 
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/game", async context =>
                {
                    // This is a sample endpoint for testing
                    await context.Response.WriteAsync("Web Game from GameFramework Startup");
                });
                // Define other endpoints your GameFramework might expose
            });
        }
    }
}
