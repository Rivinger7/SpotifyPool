using Hellang.Middleware.ProblemDetails;
using BusinessLogicLayer.Implement.CustomExceptions;
using Microsoft.AspNetCore.Mvc;
using Utility.Coding;
using Microsoft.AspNetCore.Identity;
using BusinessLogicLayer.Mapper.Mappers;

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

        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // API Config
            services.ConfigRoute();
            services.AddSwaggerGen();
            services.AddCors();

            // Problem Details
            services.AddProblemDetails();

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

        public static void AddProblemDetails(this IServiceCollection services)
        {
            services.AddProblemDetails(options =>
            {
                // Đảm bảo chi tiết lỗi không được trả về cho client
                options.IncludeExceptionDetails = (ctx, ex) => false;

                // Cấu hình trước khi trả về Response
                options.OnBeforeWriteDetails = (ctx, details) =>
                {
                    // Khởi tạo môi trường hiện tại (PaymentModel, Development, Production)
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
