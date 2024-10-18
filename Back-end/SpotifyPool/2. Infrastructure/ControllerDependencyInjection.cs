using Microsoft.AspNetCore.Identity;

namespace SpotifyPool.Infrastructure
{
    public static class ControllerDependencyInjection
    {
        // Static logger instance
        private static readonly ILogger _logger;

        static ControllerDependencyInjection()
        {
            // Initialize the logger
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); // You can add other logging providers like file, etc.
            });
            _logger = loggerFactory.CreateLogger("DependencyInjectionLogger");
        }

        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // API Config
            services.ConfigRoute();
            services.AddSwaggerGen();
            services.AddCors();

            // Log an informational message
            _logger.LogInformation("Controllers have been configured.");
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

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 1;
            });
        }
        public static void AddCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost:5173") // Or using AllowAnyOrigin() for all
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });
        }
    }
}
