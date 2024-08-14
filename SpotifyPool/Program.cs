using MongoDB.Driver;
using Data_Access_Layer.DBContext;
using Data_Access_Layer.Repositories.Accounts.Customers;
using Business_Logic_Layer.BusinessLogic;
using Data_Access_Layer.Repositories.Accounts.Authentication;
using Business_Logic_Layer.Services.EmailSender;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using SpotifyPool.JIRA_REST_API.Issues;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Config the database
builder.Services.Configure<MongoDBSetting>(builder.Configuration.GetSection("MongoDBSettings"));

//DI for the database
builder.Services.AddSingleton<SpotifyPoolDBContext>();

// Add AutoMapper configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Config the Google Identity
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie().AddGoogle(options =>
{
    options.ClientId = builder.Configuration.GetSection("Authentication:Google:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("Authentication:Google:ClientSecret").Value;
    options.CallbackPath = "/api/authentication/signin-google"; // Đường dẫn Google sẽ chuyển hướng sau khi xác thực

    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("openid");

    options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
});

// Config the email sender (SMTP)
builder.Services.Configure<EmailSenderSetting>(builder.Configuration.GetSection("Email"));

// Register Jira Cloud REST API Client
builder.Services.AddSingleton<IssueClient>();

// Register DAL services
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();

// Register BLL services
builder.Services.AddScoped<CustomerBLL>();
builder.Services.AddScoped<AuthenticationBLL>();

builder.Services.AddScoped<IEmailSenderCustom, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
