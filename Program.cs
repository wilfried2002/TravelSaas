// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TravelSaaS.Data;
using TravelSaaS.Models.Entities;
using TravelSaaS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Application services
builder.Services.AddScoped<DataInitializer>();
builder.Services.AddScoped<AuthService>();

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// IMPORTANT: Do NOT re-add the Identity cookie scheme; Identity already registers it.
// Just add JWT for APIs and configure the existing Identity cookie options.

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        var secretKey = jwtSettings["SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("SecretKey is not configured in appsettings.json");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Configure existing Identity cookie (do not AddCookie with Identity.Application again)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/SuperAdminLogin";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("GlobalAdminOnly", policy => policy.RequireRole("AgencyGlobalAdmin"));
    options.AddPolicy("PointAdminOnly", policy => policy.RequireRole("AgencyPointAdmin"));
    options.AddPolicy("OperatorOnly", policy => policy.RequireRole("AgencyOperator"));
    options.AddPolicy("AgencyScoped", policy => policy.RequireClaim("AgencyId"));
    options.AddPolicy("AdminOrAbove", policy => 
        policy.RequireRole("SuperAdmin", "AgencyGlobalAdmin", "AgencyPointAdmin"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize database and roles
try
{
    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<DataInitializer>();
        await initializer.InitializeAsync();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Une erreur s'est produite lors de l'initialisation de la base de données.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
