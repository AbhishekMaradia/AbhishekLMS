using FluentValidation;
using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Auth.Services;
using LMS_SoulCode.Features.Auth.Validators;
using LMS_SoulCode.RepositoryMapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Infrastructure;
using LMS_SoulCode.Features.UserPermissions.Mappings;
using LMS_SoulCode.Features.Common.Utilities;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning() // Only log warnings and errors to reduce I/O
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, 
        buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(30)) // Buffer logs
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers(options =>
{
    options.MaxModelValidationErrors = 50; // Limit validation errors
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Field is required");
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.MaxDepth = 32; // Reduce from 64 to 32
});

// Configure file upload limits for large video files
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 1_610_612_736; // 1.5 GB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 1_610_612_736; // 1.5 GB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Add request timeout and performance optimizations
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1_610_612_736; // 1.5 GB
    options.Limits.MinRequestBodyDataRate = null; // Disable min data rate to prevent timeouts on slow uploads
    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.MaxConcurrentUpgradedConnections = 100;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(120);
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LMS_SoulCode API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT Bearer token"
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
    c.OperationFilter<FileUploadOperationFilter>();

});
builder.Services.AddAuthorization();  

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit cache size
    options.CompactionPercentage = 0.25; // Compact when 75% full
});

builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"), sqlOptions =>
    {
        sqlOptions.CommandTimeout(30); // Reduce command timeout
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    })
    .EnableSensitiveDataLogging(false) // Disable in production
    .EnableServiceProviderCaching()); // Cache service provider

builder.Services.AddRepoServiceMapping();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"]!;
var issuer = jwt["Issuer"]!;
var audience = jwt["Audience"]!;

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddScoped<DatabaseSeeder>(); // Register Seeder

builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    // Add development policy for Swagger
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Support token in query string for video streaming and downloads
app.Use(async (context, next) =>
{
    if (context.Request.Query.ContainsKey("access_token"))
    {
        var token = context.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(token))
        {
            context.Request.Headers.Authorization = $"Bearer {token}";
        }
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.InjectStylesheet("/swagger-ui/custom.css");
        c.InjectJavascript("/swagger-ui/custom.js");
    });
}


app.UseStaticFiles();
app.UseDefaultFiles();

// Use different CORS policy based on environment
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentPolicy"); // Allow all origins in development
}
else
{
    app.UseCors("OpenPolicy"); // Restricted origins in production
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Attach Frontend: Map all non-API routes to index.html
app.MapFallbackToFile("index.html");

// // Seed Database
// using (var scope = app.Services.CreateScope())
// {
//     var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
//     await seeder.SeedAsync();
// }

app.Run();
