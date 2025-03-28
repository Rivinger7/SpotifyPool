using BusinessLogicLayer.DependencyInjection.Dependency_Injections;
using BusinessLogicLayer.Implement.Services.SignalR.Playlists;
using BusinessLogicLayer.Implement.Services.SignalR.StreamCounting;
using Google.Protobuf.WellKnownTypes;
using Hellang.Middleware.ProblemDetails;
using SpotifyPool.GraphQL.Authentication;
using SpotifyPool.GraphQL.Playlists;
using SpotifyPool.GraphQL.Query;
using SpotifyPool.GraphQL.Tracks;
using SpotifyPool.Infrastructure;
using SpotifyPool.Infrastructure.EnvironmentVariable;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

EnvironmentVariableLoader.LoadEnvironmentVariable();

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
//builder.Services.AddEndpointsApiExplorer();

// Dependency Injections
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBusinessInfrastructure(builder.Configuration);

builder.Services.AddGraphQLServer().AddAuthorization()
    .AddQueryType<QueryInitialization>()
    .AddTypeExtension<TrackQueryType>()
    .AddTypeExtension<PlaylistQueryType>()
    .AddMutationType<AuthenticationMutationType>();


var app = builder.Build();

app.UseProblemDetails();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowSpecificOrigin");

app.MapControllers();

app.MapGraphQL("/graphql");

app.MapHub<StreamCountingHub>($"{Environment.GetEnvironmentVariable("SPOTIFYPOOL_HUB_COUNT_STREAM_URL")}");
app.MapHub<PlaylistHub>($"{Environment.GetEnvironmentVariable("SPOTIFYPOOL_HUB_PLAYLIST_URL")}");

app.Run();