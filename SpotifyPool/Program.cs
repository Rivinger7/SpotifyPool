using SpotifyPool.Data;
using SpotifyPool.Models;
using SpotifyPool.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<TestUserWithDBServices>();

//Config the database
builder.Services.Configure<SpotifyPoolDatabaseSettings>(builder.Configuration.GetSection("MongoDBSettings"));
//DI for the database
builder.Services.AddSingleton<SpotifyPoolDBContext>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
