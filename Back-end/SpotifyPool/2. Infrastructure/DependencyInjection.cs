using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Hellang.Middleware.ProblemDetails;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.DependencyInjection.Dependency_Injections;
using Microsoft.AspNetCore.Mvc;
using Utility.Coding;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using BusinessLogicLayer.Implement.Services.Authentication;
using BusinessLogicLayer.Implement.Services.JWTs;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Implement.Microservices.EmailSender;
using DataAccessLayer.Repository.Entities;
using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.Interface.Microservices_Interface.EmailSender;
using BusinessLogicLayer.Implement.Microservices.JIRA_REST_API.Issues;
using BusinessLogicLayer.Setting.Microservices.EmailSender;
using BusinessLogicLayer.Interface.Services_Interface.JWTs;
using DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool;
using BusinessLogicLayer.Setting.Microservices.Cloudinaries;

namespace SpotifyPool.Infrastructure
{
    public static class DependencyInjection
    {
        // Static logger instance
        private static readonly ILogger _logger;

        static DependencyInjection()
        {
            // Initialize the logger
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); // You can add other logging providers like file, etc.
            });
            _logger = loggerFactory.CreateLogger("DependencyInjectionLogger");
        }

        public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigRoute();
            services.AddSwaggerGen();
            services.AddCors();
            services.AddDatabase(configuration);
            services.AddAutoMapper();
            services.AddInfrastructure(configuration);
            services.AddServices(configuration);
            services.AddProblemDetails();
            services.AddCloudinary(configuration);

            // Log an informational message
            _logger.LogInformation("Services have been configured.");

            // Config Session HtppContext
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
        }

        public static void AddCloudinary(this IServiceCollection services, IConfiguration configuration)
        {
            // Set your Cloudinary credentials
            //=================================
            //DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            //Cloudinary cloudinary = new(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            //cloudinary.Api.Secure = true;

            services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
            services.AddScoped<CloudinaryService>();
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
            //services.AddScoped<CustomerBLL>();
            services.AddScoped<IAuthenticationBLL, AuthenticationBLL>();

            services.AddScoped<IEmailSenderCustom, EmailSender>();
            // Config the email sender (SMTP)
            services.Configure<EmailSenderSetting>(configuration.GetSection("Email"));

            // Register Jira Cloud REST API Client
            services.AddSingleton<IssueClient>();

            services.AddTransient<IJwtBLL, JwtBLL>();

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
                        Array.Empty<string>()
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

            // Add Identity
            services.AddIdentity<User, Roles>()
                .AddMongoDbStores<User, Roles, ObjectId>
                (
                    configuration.GetSection("MongoDBSettings:ConnectionString").Value,
                    configuration.GetSection("MongoDBSettings:DatabaseName").Value
                )
                .AddDefaultTokenProviders();

        }

        public static void AddProblemDetails(this IServiceCollection services)
        {
            services.AddProblemDetails(options =>
            {
                // Đảm bảo chi tiết lỗi không được trả về cho client
                options.IncludeExceptionDetails = (ctx, ex) => false;

                // Cấu hình trước khi trả về Response
                options.OnBeforeWriteDetails = (ctx, details) =>
                {
                    // Khởi tạo môi trường hiện tại (Testing, Development, Production)
                    //IHostEnvironment env = ctx.RequestServices.GetRequiredService<IHostEnvironment>();

                    //// Inject logger
                    //var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();

                    // Tạo TraceId
                    var traceId = ctx.TraceIdentifier;
                    details.Extensions["traceId"] = traceId;

                    // Log thông tin chi tiết của lỗi
                    _logger.LogError($"Error occurred. TraceId: {traceId}, StatusCode: {details.Status}, Title: {details.Title}, Error: {details.Detail}");
                };

                // Map các lỗi cụ thể từ custom exception
                options.Map<ArgumentNullCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = Util.GetTitleCustomException(ex.ParamName, "Null Reference"),
                        Status = ex.StatusCode,
                        Detail = ex.Message,
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<CustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = ex.Title,
                        Status = ex.StatusCode,
                        Detail = ex.Message,
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<BadRequestCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = Util.GetTitleCustomException(ex.Title, "Bad Rquest"),
                        Status = StatusCodes.Status400BadRequest,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<BusinessRuleViolationCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Business Rule Violation",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<ConcurrencyCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Concurrency",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<DataExistCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Conflict",
                        Status = StatusCodes.Status409Conflict,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<DataNotFoundCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Data Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<ForbbidenCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Forbbiden",
                        Status = StatusCodes.Status403Forbidden,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<InternalServerErrorCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Internal Server Error",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<InvalidDataCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Invalid Data",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<RequestTimeoutCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Request Timeout",
                        Status = StatusCodes.Status408RequestTimeout,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<ServiceUnavailableCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Service Unavailable",
                        Status = StatusCodes.Status503ServiceUnavailable,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                options.Map<UnAuthorizedCustomException>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = Util.GetTitleCustomException(ex.Title, "Unauthorized"),
                        Status = StatusCodes.Status401Unauthorized,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });

                // Xử lý lỗi chung chung, không bắt được loại lỗi cụ thể
                //options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
                options.Map<Exception>(ex =>
                {
                    ProblemDetails details = new()
                    {
                        Title = "Internal Server Error",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = ex.Message
                    };

                    // Hiển thị chi tiết lỗi đầy đủ trong môi trường Development
                    _logger.LogError($"Full error details: {ex}");

                    return details;
                });
            });
        }
    }
}
