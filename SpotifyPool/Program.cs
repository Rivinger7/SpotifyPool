using MongoDB.Driver;
using Data_Access_Layer.DBContext;
using Data_Access_Layer.Repositories.Accounts.Customers;
using Business_Logic_Layer.BusinessLogic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DAL services
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Register BLL services
builder.Services.AddScoped<CustomerBLL>();

//Config the database
builder.Services.Configure<MongoDBSetting>(builder.Configuration.GetSection("MongoDBSettings"));

//DI for the database
builder.Services.AddSingleton<SpotifyPoolDBContext>();

// Add AutoMapper configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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
