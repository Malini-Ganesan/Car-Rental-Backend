using System.Security.Claims;
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

// ──────────────────────────────────────────────────────────────────
// ✅ STEP 1: Read all config from Environment Variables at runtime
//    These are injected by Kubernetes via ConfigMap + Secret
// ──────────────────────────────────────────────────────────────────
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
    ?? throw new InvalidOperationException("DB_PASSWORD environment variable is not set.");
var keycloakUrl = Environment.GetEnvironmentVariable("KEYCLOAK_URL") ?? "http://keycloak:8080";
var imageBaseUrl = Environment.GetEnvironmentVariable("IMAGE_BASE_URL") ?? "http://backend:5020";
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://frontend:80";
var keycloakRealm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? "CarRentalRealm";

// ✅ Build the full issuer URL once — used in both JWT validation and CORS
var keycloakIssuer = $"{keycloakUrl}/realms/{keycloakRealm}";

// ──────────────────────────────────────────────────────────────────
// ✅ STEP 2: Override appsettings.json values with runtime env vars
//    .NET does NOT auto-substitute ${VAR} syntax in appsettings.json
//    So we override the config keys programmatically here
// ──────────────────────────────────────────────────────────────────
builder.Configuration["ConnectionStrings:DefaultConnection"] =
    $"Host={dbHost};Port=5432;Database=carrentaldb;Username=postgres;Password={dbPassword}";

builder.Configuration["Jwt:Authority"] = keycloakIssuer;
builder.Configuration["Jwt:Audience"] = "car-rental-app";
builder.Configuration["Jwt:RequireHttpsMetadata"] = "false";
builder.Configuration["ImageStorage:BaseUrl"] = imageBaseUrl;

// ──────────────────────────────────────────────────────────────────
// ✅ STEP 3: Database — uses the overridden connection string above
// ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ──────────────────────────────────────────────────────────────────
// Dependency Injection — unchanged
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddHttpClient<NodeRedService>();

// ──────────────────────────────────────────────────────────────────
// ✅ STEP 4: Keycloak JWT Authentication
//    ValidIssuers now uses the runtime keycloakIssuer variable
//    instead of hardcoded localhost/keycloakContainer values
// ──────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // ✅ Authority comes from env var — no more hardcoded localhost:8080
        options.Authority = keycloakIssuer;
        options.Audience = builder.Configuration["Jwt:Audience"];

        var requireHttps = builder.Configuration["Jwt:RequireHttpsMetadata"];
        options.RequireHttpsMetadata = bool.TryParse(requireHttps, out var parsed) && parsed;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,

            // ✅ FIXED: ValidIssuers now built from env var
            //    Previously hardcoded "localhost:8080" and "keycloakContainer:8080"
            //    Now it accepts both the K8s service URL and external URL for the same realm
            ValidIssuers = new[]
            {
                keycloakIssuer,                                        // http://keycloak:8080/realms/CarRentalRealm
                $"http://localhost:8080/realms/{keycloakRealm}",       // local dev fallback
                $"https://your-domain.com/realms/{keycloakRealm}"     // production domain — update as needed
            },

            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        // ✅ Role extraction from Keycloak realm_access claim — unchanged logic
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
                            identity?.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                    }
                }
                return Task.CompletedTask;
            },

            // ✅ ADDED: Log auth failures to help debug token/issuer mismatches in K8s
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[JWT] Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                Console.WriteLine($"[JWT] Challenge issued: {context.Error} - {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ──────────────────────────────────────────────────────────────────
// ✅ STEP 5: CORS — origins from env vars, not hardcoded
//    Previously: hardcoded "http://localhost:4200" and "http://frontend:80"
//    Now: reads FRONTEND_URL from ConfigMap
// ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            // ✅ frontendUrl comes from FRONTEND_URL env var (set in ConfigMap)
            .WithOrigins(
                frontendUrl,                   // e.g. http://frontend:80  (K8s internal)
                "http://localhost:4200"         // local dev only
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ✅ Bind to all interfaces so K8s can reach the container
builder.WebHost.UseUrls("http://0.0.0.0:5020");

var app = builder.Build();

// ──────────────────────────────────────────────────────────────────
// ✅ STEP 6: Auto-run DB migrations on startup
//    Uncommented and wrapped in retry logic for K8s startup races
//    (postgres pod may not be ready yet when backend starts)
// ──────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var retries = 5;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();   // ✅ Re-enabled
            Console.WriteLine("[DB] Migrations applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            Console.WriteLine($"[DB] Migration failed ({retries} retries left): {ex.Message}");
            Thread.Sleep(3000);      // wait 3s before retrying
        }
    }
}

// ──────────────────────────────────────────────────────────────────
// Middleware pipeline — order matters, do not rearrange
// ──────────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseCors("AllowAngular");        // ✅ Must be before UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();