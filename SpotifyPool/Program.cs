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
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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
						new string[] {}
					}
				});
});

//Config the database
builder.Services.Configure<MongoDBSetting>(builder.Configuration.GetSection("MongoDBSettings"));

//DI for the database
builder.Services.AddSingleton<SpotifyPoolDBContext>();

// Add AutoMapper configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Config the Google Identity
builder.Services.AddAuthentication(options =>
{
	//options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	////options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
	//options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddCookie().AddGoogle(options =>
{
	options.ClientId = builder.Configuration.GetSection("Authentication:Google:ClientId").Value;
	options.ClientSecret = builder.Configuration.GetSection("Authentication:Google:ClientSecret").Value;
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
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:SecretKey"])),

		ClockSkew = TimeSpan.Zero
	};
});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("GoogleOrJwt", policy =>
	{
		policy.AddAuthenticationSchemes(GoogleDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
		policy.RequireAuthenticatedUser();
	});
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

// Config Session HtppContext
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseSwagger();
//app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
