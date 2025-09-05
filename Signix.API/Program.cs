using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;
using SharedKernel.AuthorizeHandler;
using SharedKernel.Models;
using SharedKernel.Services;
using Signix.API.Infrastructure;
using Signix.API.Infrastructure.Messaging;
using Signix.Entities.Context;
using System.Text.Json.Serialization;
using static Signix.API.Models.Meta;

var builder = WebApplication.CreateBuilder(args);

#region Authorize Configuration

builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.ConfigureAuthorization();
builder.Services.AddScoped<IUser, AuthUser>();

builder.Services.AddTransient<TokenHandler>();
#endregion

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // your frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to handle circular references
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<SignixDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "PostgreSQL connection string 'DefaultConnection' not found. " +
            "Please check your appsettings.json file.");
    }

    options.UseNpgsql(connectionString);

    // Enable detailed errors in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// Register RabbitMQ service
try
{
    builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQSettings"));
    builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
    Console.WriteLine("🐰 RabbitMQ service registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Warning: Failed to register RabbitMQ service: {ex.Message}");
    Console.WriteLine("📝 Note: RabbitMQ features will be disabled. Please ensure RabbitMQ server is running.");

    // Register a null implementation or mock service if needed for development
    // builder.Services.AddSingleton<IRabbitMqService, NullRabbitMqService>();
}

// Register services
//builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<ISigningRoomService, SigningRoomService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ISignerService, SignerService>();

builder.Services.AddHostedService<AckConsumerService>();

// Add OpenAPI/Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Signix API",
        Version = "v1",
        Description = "API for managing digital signing rooms, documents, and signers with RabbitMQ integration",
    });

    // Include XML comments if you have them
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // if (File.Exists(xmlPath))
    //     c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Signix API V1");
        c.RoutePrefix = string.Empty; // This makes Swagger UI available at the root URL
        c.DocumentTitle = "Signix API Documentation";
        c.DisplayRequestDuration();
    });

    // Database initialization in development
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<SignixDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("🔌 Initializing PostgreSQL database...");

            // This will create the database if it doesn't exist
            // Note: The postgres user must have CREATEDB privileges
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("✅ Database created/verified successfully!");

            // Apply any pending migrations (in case you add them later)
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("🔄 Applying pending migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("✅ Migrations applied successfully!");
            }
            else
            {
                logger.LogInformation("✅ Database is up to date!");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Database initialization failed: {Message}", ex.Message);
            logger.LogError("🔧 Please check your PostgreSQL connection and ensure:");
            logger.LogError("   1. PostgreSQL is running");
            logger.LogError("   2. Connection string is correct in appsettings.json");
            logger.LogError("   3. PostgreSQL user has CREATEDB privileges");
            logger.LogError("   4. User has proper permissions");

            // Try manual database creation guidance
            logger.LogWarning("💡 Alternative: Create database manually in pgAdmin:");
            logger.LogWarning("   1. Open pgAdmin");
            logger.LogWarning("   2. Right-click server → Create → Database");
            logger.LogWarning("   3. Name: 'SignixDb'");
            logger.LogWarning("   4. Restart the application");

            throw;
        }
    }
}
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// PSEUDOCODE:
// 1. Determine base directory (parent of current working directory).
// 2. Compose absolute paths for both folders: TagedPdf and SignedPdf.
// 3. Ensure both directories exist (Directory.CreateDirectory is idempotent).
// 4. Register two Static File middlewares:
//    - /TagedPdf -> physical TagedPdf folder
//    - /SignedPdf -> physical SignedPdf folder
// 5. Replace previous single app.UseStaticFiles call.

// STATIC FILE MIDDLEWARE (replaces previous single TagedPdf block)
var baseFilesDir = Directory.GetParent(Directory.GetCurrentDirectory())!.FullName;
var taggedPdfPath = Path.Combine(baseFilesDir, "TagedPdf");
var taggedPdfPath1 = Path.Combine(baseFilesDir, "TagedPdf1");
var taggedPdfPath2 = Path.Combine(baseFilesDir, "TagedPdf2");
var signedPdfPath = Path.Combine(baseFilesDir, "SignedPdf");
var signedPdfPath1 = Path.Combine(baseFilesDir, "SignedPdf1");
var signedPdfPath2 = Path.Combine(baseFilesDir, "SignedPdf2");

// Ensure folders exist
Directory.CreateDirectory(taggedPdfPath);
Directory.CreateDirectory(signedPdfPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(taggedPdfPath),
    RequestPath = "/TagedPdf"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(taggedPdfPath1),
    RequestPath = "/TagedPdf1"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(taggedPdfPath2),
    RequestPath = "/TagedPdf2"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(signedPdfPath),
    RequestPath = "/SignedPdf"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(signedPdfPath1),
    RequestPath = "/SignedPdf1"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(signedPdfPath2),
    RequestPath = "/SignedPdf2"
});

app.UseAuthorization();

app.MapControllers();

app.Run();

// Seed method for initial data