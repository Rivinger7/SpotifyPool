using BusinessLogicLayer.DependencyInjection.Dependency_Injections;
using BusinessLogicLayer.Implement.Services.SignalR.Playlists;
using BusinessLogicLayer.Implement.Services.SignalR.StreamCounting;
using Hellang.Middleware.ProblemDetails;
using SpotifyPool.Infrastructure;
using SpotifyPool.Infrastructure.EnvironmentVariable;

var builder = WebApplication.CreateBuilder(args);

EnvironmentVariableLoader.LoadEnvironmentVariable();

// Chỉ lắng nghe trên một cổng (ví dụ HTTP)
//builder.WebHost.UseUrls("https://localhost:7018");

// Cấu hình cổng lắng nghe từ môi trường, thay vì hard-code
//var port = Environment.GetEnvironmentVariable("PORT") ?? "7018";
//builder.WebHost.UseUrls($"https://localhost:{port}");

// Lấy cổng từ biến môi trường hoặc để trống để chọn ngẫu nhiên
//var port = Environment.GetEnvironmentVariable("PORT");
//var protocol = builder.Environment.IsDevelopment() ? "http" : "https";

// Nếu không có biến môi trường PORT, để trống UseUrls để ứng dụng tự chọn cổng ngẫu nhiên
//if (string.IsNullOrEmpty(port))
//{
//    builder.WebHost.UseUrls($"{protocol}://localhost"); // Không chỉ định cổng
//}
//else
//{
//    builder.WebHost.UseUrls($"{protocol}://localhost:{port}"); // Sử dụng cổng từ biến môi trường
//}

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
//Util.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

app.UseProblemDetails();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Đặt tiêu đề
    c.DocumentTitle = "SpotifyPool API";

    // Đường dẫn đến file JSON của Swagger
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SpotifyPool API V1");

    // Inject JavaScript để chuyển đổi theme
    c.InjectJavascript("/theme-switcher.js");
});

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseSession();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowSpecificOrigin");

app.MapControllers();

app.MapHub<StreamCountingHub>($"{Environment.GetEnvironmentVariable("SPOTIFYPOOL_HUB_COUNT_STREAM_URL")}");
app.MapHub<PlaylistHub>($"{Environment.GetEnvironmentVariable("SPOTIFYPOOL_HUB_PLAYLIST_URL")}");

app.Run();