using Business_Logic_Layer.BusinessLogic;
using Business_Logic_Layer.Services.EmailSender;
using Data_Access_Layer.DBContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using SpotifyPool.JIRA_REST_API.Issues;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace SpotifyPool.Main
{
    public static class DependencyInjection
    {
        public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigRoute();
            services.AddDatabase(configuration);
            //services.AddAutoMapper();

            // Config Session HtppContext
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
        }

        public static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
        }

        // Config the database
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDBSetting>(configuration.GetSection("MongoDBSettings"));
            services.AddSingleton<SpotifyPoolDBContext>();
        }

        // Add AutoMapper configuration
        public static void AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }

        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register BLL services
            services.AddScoped<CustomerBLL>();
            services.AddScoped<AuthenticationBLL>();

            services.AddScoped<IEmailSenderCustom, EmailSender>();
            // Config the email sender (SMTP)
            services.Configure<EmailSenderSetting>(configuration.GetSection("Email"));

            // Register Jira Cloud REST API Client
            services.AddSingleton<IssueClient>();

            // Config JWT
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Spotify Pool", Version = "v1" });

                // Add JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your token",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Config the Google Identity
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddCookie("Cookies").AddGoogle(options =>
            {
                options.ClientId = configuration.GetSection("Authentication:Google:ClientId").Value;
                options.ClientSecret = configuration.GetSection("Authentication:Google:ClientSecret").Value;
                options.CallbackPath = "/api/authentication/signin-google"; // Đường dẫn Google sẽ chuyển hướng sau khi xác thực

                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("openid");

                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
            }).AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    //tự cấp token
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,

                    //ký vào token
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:SecretKey"])),

                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("GoogleOrJwt", policy =>
                {
                    policy.AddAuthenticationSchemes(GoogleDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });

        }

    }
}
