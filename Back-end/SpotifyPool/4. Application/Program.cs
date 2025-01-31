using Hellang.Middleware.ProblemDetails;
using SpotifyPool.Infrastructure;
using System.Diagnostics;
using BusinessLogicLayer.DependencyInjection.Dependency_Injections;
using SpotifyPool.Infrastructure.EnvironmentVariable;
using Microsoft.AspNetCore.HttpOverrides;
using BusinessLogicLayer.Implement.Services.SignalR.StreamCounting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BusinessLogicLayer.Implement.Services.SignalR.Playlists;
using BusinessLogicLayer.Implement.Services.SignalR.PlaybackSync;

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


// Real-time IP Address
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Uncomment and set your trusted proxies if your app is behind a proxy
    // options.KnownProxies.Add(IPAddress.Parse("::ffff:127.0.0.1"));
    // Or use KnownNetworks for trusted network ranges if you use multiple proxies
    // options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("192.168.1.0"), 24));
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

// Đăng ký middleware lấy địa chỉ IP
//app.UseMiddleware<IpAddressMiddleware>();

app.UseHttpsRedirection();

app.UseSession();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowSpecificOrigin");

app.MapControllers();

app.MapHub<StreamCountingHub>($"{Environment.GetEnvironmentVariable("SPOTIFYPOOL_HUB_COUNT_STREAM_URL")}");
app.MapHub<PlaylistHub>($"{Environment.GetEnvironmentVariable("SPOTIFYPOOL_HUB_PLAYLIST_URL")}");
app.MapHub<PlaybackSyncHub>($"{Environment.GetEnvironmentVariable("SPOTIFYPOOL_HUB_PLAYBACK_SYNC_URL")}");

// Stopwatch End
stopwatch.Stop();
app.Logger.LogInformation($"Application startup completed in {stopwatch.ElapsedMilliseconds} ms");

app.Run();