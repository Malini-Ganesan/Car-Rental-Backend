using System.Security.Claims;
using System.Text;
using CarRentalAPI.Data;
using CarRentalAPI.Repositories.Interface;
using CarRentalAPI.Services.Interface;
using CarRentalAPI.Services;
using CarRentalAPI.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CarRentalAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Dependency Injection
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Keycloak JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        var requireHttps = builder.Configuration["Jwt:RequireHttpsMetadata"];
        options.RequireHttpsMetadata = bool.TryParse(requireHttps, out var parsed) && parsed;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false    ,
            ValidateIssuer = true,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;

                if (!string.IsNullOrEmpty(realmAccess))
                {
                    var roles = System.Text.Json.JsonDocument.Parse(realmAccess)
                        .RootElement.GetProperty("roles");

                    foreach (var role in roles.EnumerateArray())
                    {
                        var roleValue = role.GetString();

                        if (!string.IsNullOrEmpty(roleValue))
                        {
                            identity?.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                        }
                    }
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<NodeRedService>();

//cors(angular app)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();