using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Signix.API.Infrastructure;
using Signix.Entities.Context;
using System.Text.Json.Serialization;
using SharedKernel.AuthorizeHandler;
using Microsoft.Identity.Web;
using SharedKernel.Services;
using SharedKernel.Models;

var builder = WebApplication.CreateBuilder(args);

#region Authorize Configuration

builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.ConfigureAuthorization();
builder.Services.AddScoped<IUser, AuthUser>();

builder.Services.AddTransient<TokenHandler>();
#endregion

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

// Register services
builder.Services.AddScoped<ISigningRoomService, SigningRoomService>();

// Add OpenAPI/Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Signix API",
        Version = "v1",
        Description = "API for managing digital signing rooms and documents",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Signix Team",
            Email = "support@signix.com"
        }
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
    
    // Test database connection and apply migrations in development
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<SignixDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("?? Testing PostgreSQL connection...");
            
            // Test the connection
            await context.Database.CanConnectAsync();
            logger.LogInformation("? PostgreSQL connection successful!");
            
            // Apply any pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("?? Applying pending migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("? Migrations applied successfully!");
            }
            else
            {
                logger.LogInformation("? Database is up to date!");
            }
            
            // Seed some initial data if database is empty
            await SeedDatabaseAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? Database connection or migration failed: {Message}", ex.Message);
            logger.LogError("?? Please check your PostgreSQL connection and ensure:");
            logger.LogError("   1. PostgreSQL is running");
            logger.LogError("   2. Connection string is correct in appsettings.json");
            logger.LogError("   3. Database 'SignixDb' exists");
            logger.LogError("   4. User has proper permissions");
            throw;
        }
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Console.WriteLine("?? Signix API is ready!");
Console.WriteLine("?? Using PostgreSQL Database");
Console.WriteLine("?? Swagger UI: Available at the root URL when running in Development");
Console.WriteLine("?? API Base URL: /api/signing-rooms");

app.Run();

// Seed method for initial data
static async Task SeedDatabaseAsync(SignixDbContext context, ILogger logger)
{
    try
    {
        // Check if we already have data
        if (await context.Users.AnyAsync() || await context.SigningRooms.AnyAsync())
        {
            logger.LogInformation("?? Database already contains data, skipping seed.");
            return;
        }

        logger.LogInformation("?? Seeding initial data...");

        // Add sample users (notaries)
        var users = new[]
        {
            new Signix.Entities.Entities.User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@notary.com",
                MetaData = System.Text.Json.JsonDocument.Parse("{}").RootElement
            },
            new Signix.Entities.Entities.User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@notary.com",
                MetaData = System.Text.Json.JsonDocument.Parse("{}").RootElement
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Add sample clients
        var clients = new[]
        {
            new Signix.Entities.Entities.Client
            {
                Name = "Acme Corporation",
                Description = "Technology company",
                AzureClientId = "sample-azure-client-id-1",
                ClientSecret = System.Text.Json.JsonDocument.Parse("{}").RootElement,
                CreatedBy = 1,
                ModifiedBy = 1,
                IsActive = true
            }
        };

        context.Clients.AddRange(clients);
        await context.SaveChangesAsync();

        // Add sample signing rooms
        var signingRooms = new[]
        {
            new Signix.Entities.Entities.SigningRoom
            {
                Name = "NDA Signing Session",
                Description = "Non-disclosure agreement signing",
                NotaryId = users[0].Id,
                CreatedBy = 1,
                ModifiedBy = 1,
                StatusId = 1,
                CreatedAt = DateTime.UtcNow,
                MetaData = System.Text.Json.JsonDocument.Parse("{}").RootElement
            },
            new Signix.Entities.Entities.SigningRoom
            {
                Name = "Contract Signing",
                Description = "Service agreement contract signing",
                NotaryId = users[1].Id,
                CreatedBy = 1,
                ModifiedBy = 1,
                StatusId = 1,
                CreatedAt = DateTime.UtcNow,
                MetaData = System.Text.Json.JsonDocument.Parse("{}").RootElement
            }
        };

        context.SigningRooms.AddRange(signingRooms);
        await context.SaveChangesAsync();

        logger.LogInformation("? Database seeded successfully!");
        logger.LogInformation($"?? Created {users.Length} users");
        logger.LogInformation($"?? Created {clients.Length} clients");
        logger.LogInformation($"?? Created {signingRooms.Length} signing rooms");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "? Error seeding database: {Message}", ex.Message);
    }
}
