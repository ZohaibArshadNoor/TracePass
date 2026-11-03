
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using TracePass.Application.Authentication;
using TracePass.Application.Common.Interfaces;
using TracePass.Application.Organizations;
using TracePass.Application.Users;
using TracePass.Infrastructure.Configuration;
using TracePass.Infrastructure.Data;
using TracePass.Infrastructure.Data.Seeding;
using TracePass.Infrastructure.Identity;
using TracePass.Infrastructure.Security;
using TracePass.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    }); builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TracePass API",
        Version = "v1",
        Description = "Digital Product Passport & Compliance Platform API for Garment and Textile Exporters."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience) ||
    string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException(
        "JWT issuer, audience, and secret key must be configured.");
}

var signingKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

if (signingKey.Length < 32)
{
    throw new InvalidOperationException(
        "The JWT secret key must contain at least 32 bytes.");
}

builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtSettings));
builder.Services.AddSingleton(jwtSettings);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKey),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorOnly", policy =>
        policy.RequireRole(TracePassRoles.Administrator));

    options.AddPolicy("ManufacturerOnly", policy =>
        policy.RequireRole(TracePassRoles.Manufacturer));

    options.AddPolicy("SupplierOnly", policy =>
        policy.RequireRole(TracePassRoles.Supplier));

    options.AddPolicy("DistributorOnly", policy =>
        policy.RequireRole(TracePassRoles.Distributor));

    options.AddPolicy("BuyerOnly", policy =>
        policy.RequireRole(TracePassRoles.Buyer));

    options.AddPolicy("AuditorOnly", policy =>
        policy.RequireRole(TracePassRoles.Auditor));
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

builder.Services.AddScoped<
    ITracePassAuthenticationService,
    TracePassAuthenticationService>();

builder.Services.AddScoped<
    IOrganizationService,
    OrganizationService>();

builder.Services.AddScoped<
    IUserManagementService,
    UserManagementService>();

builder.Services.AddValidatorsFromAssemblyContaining<
    OrganizationValidator>();

builder.Services.AddValidatorsFromAssemblyContaining<
    CreateUserValidator>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var app = builder.Build();

// Initialize the database's default Identity roles and administrator.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    await IdentitySeeder.SeedAsync(
        roleManager,
        userManager,
        app.Configuration);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TracePass API v1");
        options.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async (ApplicationDbContext dbContext) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync();

    return canConnect
        ? Results.Ok(new { status = "healthy", database = "connected" })
        : Results.Problem("Database connection failed");
});

app.MapControllers();

app.Run();
