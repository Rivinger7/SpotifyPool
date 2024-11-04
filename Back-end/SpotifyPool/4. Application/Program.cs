using Hellang.Middleware.ProblemDetails;
using SpotifyPool.Infrastructure;
using System.Diagnostics;
using BusinessLogicLayer.DependencyInjection.Dependency_Injections;
using SpotifyPool.Infrastructure.EnvironmentVariable;
using Utility.Coding;
using Microsoft.AspNetCore.HttpOverrides;

// Stopwatch Start
var stopwatch = new Stopwatch();
stopwatch.Start();

var builder = WebApplication.CreateBuilder(args);

EnvironmentVariableLoader.LoadEnvironmentVariable();

// Chỉ lắng nghe trên một cổng (ví dụ HTTP)
//builder.WebHost.UseUrls("https://localhost:7018");

// Cấu hình cổng lắng nghe từ môi trường, thay vì hard-code
//var port = Environment.GetEnvironmentVariable("PORT") ?? "7018";
//builder.WebHost.UseUrls($"https://localhost:{port}");

// Lấy cổng từ biến môi trường hoặc để trống để chọn ngẫu nhiên
var port = Environment.GetEnvironmentVariable("PORT");
var protocol = builder.Environment.IsDevelopment() ? "http" : "https";

// Nếu không có biến môi trường PORT, để trống UseUrls để ứng dụng tự chọn cổng ngẫu nhiên
//if (string.IsNullOrEmpty(port))
//{
//    builder.WebHost.UseUrls($"{protocol}://localhost"); // Không chỉ định cổng
//}
//else
//{
//    builder.WebHost.UseUrls($"{protocol}://localhost:{port}"); // Sử dụng cổng từ biến môi trường
//}


// Real-time IP Address
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    //options.KnownProxies.Add(IPAddress.Parse("::ffff:127.0.0.1")); // Thay địa chỉ này bằng địa chỉ proxy nếu cần
});

// Config appsettings by env
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    // Không dùng tới appsettings.json
    //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    //.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Dependency Injections
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBusinessInfrastructure(builder.Configuration);



var app = builder.Build();



// Cấu hình IpAddressHelper với IHttpContextAccessor từ DI
Util.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

app.UseProblemDetails();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseSession();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowSpecificOrigin");

app.MapControllers();

// Stopwatch End
stopwatch.Stop();
app.Logger.LogInformation($"Application startup completed in {stopwatch.ElapsedMilliseconds} ms");

app.Run();

#region TESTING
//// ========================================================================================================= //
//// ================================================ TESTING ================================================ //
//// ========================================================================================================= //
//using Hellang.Middleware.ProblemDetails;
//using SpotifyPool.Infrastructure;
//using System.Diagnostics;
//using BusinessLogicLayer.ControllerDependencyInjection.Dependency_Injections;
//using SpotifyPool.Infrastructure.EnvironmentVariable;

//// Stopwatch Start for entire application
//var appStopwatch = new Stopwatch();
//appStopwatch.Start();

//var builder = WebApplication.CreateBuilder(args);

//// Measure time for environment variable loading
//var stopwatch = Stopwatch.StartNew();
//EnvironmentVariableLoader.LoadEnvironmentVariable();
//Console.WriteLine($"EnvironmentVariableLoader loaded in {stopwatch.ElapsedMilliseconds} ms");

//// Measure time for port configuration
//stopwatch.Restart();
//var port = Environment.GetEnvironmentVariable("PORT") ?? "7018";
//builder.WebHost.UseUrls($"https://localhost:{port}");
//Console.WriteLine($"Port configuration completed in {stopwatch.ElapsedMilliseconds} ms");

//// Config appsettings by env
//stopwatch.Restart();
//builder.Configuration
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddEnvironmentVariables();
//Console.WriteLine($"Configuration setup completed in {stopwatch.ElapsedMilliseconds} ms");

//// Add services to the container

//stopwatch.Restart();
//builder.Services.AddControllers();
//Console.WriteLine($"Controllers added in {stopwatch.ElapsedMilliseconds} ms");

//stopwatch.Restart();
//builder.Services.AddEndpointsApiExplorer();
//Console.WriteLine($"Endpoints API Explorer added in {stopwatch.ElapsedMilliseconds} ms");

//// Dependency Injections
//stopwatch.Restart();
//builder.Services.AddInfrastructure(builder.Configuration);
//Console.WriteLine($"Infrastructure services added in {stopwatch.ElapsedMilliseconds} ms");

//stopwatch.Restart();
//builder.Services.AddBusinessInfrastructure(builder.Configuration);
//Console.WriteLine($"Business infrastructure services added in {stopwatch.ElapsedMilliseconds} ms");

//var app = builder.Build();

//// UseProblemDetails middleware
//stopwatch.Restart();
//app.UseProblemDetails();
//Console.WriteLine($"ProblemDetails middleware configured in {stopwatch.ElapsedMilliseconds} ms");

//// Configure the HTTP request pipeline

//stopwatch.Restart();
//app.UseSwagger();
//app.UseSwaggerUI();
//Console.WriteLine($"Swagger and SwaggerUI configured in {stopwatch.ElapsedMilliseconds} ms");

//stopwatch.Restart();
//app.UseHttpsRedirection();
//Console.WriteLine($"HTTPS redirection configured in {stopwatch.ElapsedMilliseconds} ms");

//stopwatch.Restart();
//app.UseSession();
//Console.WriteLine($"Session configured in {stopwatch.ElapsedMilliseconds} ms");

//stopwatch.Restart();
//app.UseAuthentication();
//app.UseAuthorization();
//Console.WriteLine($"Authentication and Authorization configured in {stopwatch.ElapsedMilliseconds} ms");

//stopwatch.Restart();
//app.UseCors("AllowSpecificOrigin");
//Console.WriteLine($"CORS policy configured in {stopwatch.ElapsedMilliseconds} ms");

//stopwatch.Restart();
//app.MapControllers();
//Console.WriteLine($"Controller mapping completed in {stopwatch.ElapsedMilliseconds} ms");

//// Stopwatch End for entire application
//appStopwatch.Stop();
//app.Logger.LogInformation($"Application startup completed in {appStopwatch.ElapsedMilliseconds} ms");

//app.Run();
#endregion