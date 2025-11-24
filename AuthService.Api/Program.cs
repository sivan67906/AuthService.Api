using AuthService.Application.Common.Behaviors;
using AuthService.Domain.Constants;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Persistence;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuthService API",
        Version = "v1",
        Description = "Authentication & Authorization API with RBAC"
    });

    // Enhanced custom schema ID generation to handle complex generic types
    c.CustomSchemaIds(type =>
    {
        // Get the full type name
        var fullName = type.FullName ?? type.Name;

        // Simplify namespace prefixes
        fullName = fullName
            .Replace("AuthService.Application.Features.", "App.")
            .Replace("AuthService.Infrastructure.Services.", "Infra.")
            .Replace("AuthService.Domain.Entities.", "Domain.")
            .Replace("AuthService.Api.", "Api.");

        // Handle generic types
        if (type.IsGenericType)
        {
            // Get the generic type definition name without the `1, `2, etc.
            var genericTypeName = fullName.Split('`')[0];

            // Recursively build schema IDs for generic arguments
            var genericArgs = type.GetGenericArguments()
                .Select(t => GetSchemaId(t))
                .ToArray();

            // Combine them with underscores
            return $"{genericTypeName}_{string.Join("_", genericArgs)}";
        }

        return fullName;
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// Helper method for recursive schema ID generation
static string GetSchemaId(Type type)
{
    var name = type.Name;

    // Simplify namespace for nested types
    if (type.FullName != null)
    {
        name = type.FullName
            .Replace("AuthService.Application.Features.", "")
            .Replace("AuthService.Infrastructure.Services.", "")
            .Replace("AuthService.Domain.Entities.", "")
            .Replace("AuthService.Api.", "");
    }

    // Handle generic types recursively
    if (type.IsGenericType)
    {
        var genericTypeName = name.Split('`')[0];
        var genericArgs = type.GetGenericArguments()
            .Select(t => GetSchemaId(t))
            .ToArray();
        return $"{genericTypeName}_{string.Join("_", genericArgs)}";
    }

    return name;
}

builder.Services.AddInfrastructure(builder.Configuration);

// Mapster
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(Program).Assembly);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(AuthService.Application.Features.Auth.Register.RegisterCommand).Assembly
    );
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(
    typeof(AuthService.Application.Features.Auth.Register.RegisterCommand).Assembly
);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-change-me";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

// Role-based policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(Roles.Admin));
    options.AddPolicy("RequireUserOrAdmin", policy =>
        policy.RequireRole(Roles.User, Roles.Admin));
    options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));
    options.AddPolicy("RequireFinanceAdmin", policy =>
        policy.RequireRole("FinanceAdmin", "SuperAdmin"));
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService.Api v1");
        // optional: serve swagger UI at root (https://localhost:7000/)
        // c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();