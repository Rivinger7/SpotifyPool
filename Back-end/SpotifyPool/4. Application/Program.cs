using dotenv.net;
using Hellang.Middleware.ProblemDetails;
using SpotifyPool.Infrastructure;
using System.Diagnostics;
using BusinessLogicLayer.DependencyInjection.Dependency_Injections;

// Stopwatch Start
var stopwatch = new Stopwatch();
stopwatch.Start();

var builder = WebApplication.CreateBuilder(args);

// Construct the full path to the .env file located in the "4. Application" folder
var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "4. Application", ".env");

// Load the .env file from the specific path using DotEnvOptions
var options = new DotEnvOptions(
    envFilePaths: [envFilePath],   // Pass the path to the .env file
    probeForEnv: false                     // No need to probe for .env as we are specifying the path
);
DotEnv.Load(options);

// Config appsettings by env
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddConfiguration(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseProblemDetails();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowSpecificOrigin");

app.MapControllers();

// Stopwatch End
stopwatch.Stop();
app.Logger.LogInformation($"Application startup completed in {stopwatch.ElapsedMilliseconds} ms");

app.Run();
