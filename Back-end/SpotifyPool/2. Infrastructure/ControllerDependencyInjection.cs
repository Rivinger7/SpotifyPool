using BusinessLogicLayer.Implement.CustomExceptions;
using Microsoft.AspNetCore.Identity;

namespace SpotifyPool.Infrastructure
{
    public static class ControllerDependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // API Config
            //services.ConfigRoute();
            //services.AddSwaggerGen();
            services.AddCors();
        }

        public static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            services.Configure<DataProtectionTokenProviderOptions>(otp =>
            {
                otp.TokenLifespan = TimeSpan.FromMinutes(3);
            });
        }
        public static void AddCors(this IServiceCollection services)
        {
            string clientUrl = Environment.GetEnvironmentVariable("SPOTIFYPOOL_CLIENT_URL") ?? throw new InvalidDataCustomException("SPOTIFYPOOL_CLIENT_URL is not set");

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .WithOrigins("http://localhost:5173")
                        .WithOrigins(clientUrl)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });
        }
    }
}
