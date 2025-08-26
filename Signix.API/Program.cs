using Microsoft.EntityFrameworkCore;
using Signix.API.Infrastructure;
using Signix.API.Infrastructure.Messaging;
using Signix.Entities.Context;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

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
    builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
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
builder.Services.AddScoped<ISigningRoomService, SigningRoomService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ISignerService, SignerService>();

// Add OpenAPI/Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Signix API",
        Version = "v1",
        Description = "API for managing digital signing rooms, documents, and signers with RabbitMQ integration",
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

            // Seed some initial data if database is empty
            await SeedDatabaseAsync(context, logger);
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Seed method for initial data
static async Task SeedDatabaseAsync(SignixDbContext context, ILogger logger)
{
    try
    {
        // Check if we already have data
        if (await context.Users.AnyAsync() || await context.SigningRooms.AnyAsync())
        {
            logger.LogInformation("📊 Database already contains data, skipping seed.");
            return;
        }

        logger.LogInformation("🌱 Seeding initial data...");

        // Add sample users (notaries)
        var users = new[]
        {
            new Signix.Entities.Entities.User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@notary.com",
                //MetaData = System.Text.Json.JsonDocument.Parse("{}").RootElement
            },
            new Signix.Entities.Entities.User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@notary.com",
                //MetaData = System.Text.Json.JsonDocument.Parse("{}").RootElement
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
                //ClientSecret = System.Text.Json.JsonDocument.Parse("{}").RootElement,
                CreatedBy = 1,
                ModifiedBy = 1,
                IsActive = true
            }
        };

        context.Clients.AddRange(clients);
        await context.SaveChangesAsync();

        // Add sample document statuses
        var documentStatuses = new[]
        {
            new Signix.Entities.Entities.DocumentStatus
            {
                Name = "Pending",
                Description = "Document is pending review"
            },
            new Signix.Entities.Entities.DocumentStatus
            {
                Name = "Signed",
                Description = "Document has been signed"
            },
            new Signix.Entities.Entities.DocumentStatus
            {
                Name = "Completed",
                Description = "Document has been signed and completed"
            }
        };

        context.DocumentStatuses.AddRange(documentStatuses);
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
                CreatedAt = DateTime.UtcNow,
                //MetaData = System.Text.Json.JsonDocument.Parse("{}").RootElement
            },
            new Signix.Entities.Entities.SigningRoom
            {
                Name = "Contract Signing",
                Description = "Service agreement contract signing",
                NotaryId = users[1].Id,
                CreatedBy = 1,
                ModifiedBy = 1,
                CreatedAt = DateTime.UtcNow,
                //MetaData = System.Text.Json.JsonDocument.Parse("{}").RootElement
            }
        };

        context.SigningRooms.AddRange(signingRooms);
        await context.SaveChangesAsync();

        // Add sample documents
        var documents = new[]
        {
            new Signix.Entities.Entities.Document
            {
                Name = "NDA_Template.pdf",
                Description = "Standard non-disclosure agreement template",
                ClientId = clients[0].Id,
                FileSize = 524288, // 512KB
                FileType = "application/pdf",
                DocTags = JsonSerializer.Serialize(new
                {
                    document_type = "NDA",
                    priority = "high",
                    category = "legal",
                    confidentiality_level = "strict",
                    expiry_date = "2024-12-31",
                    department = "legal"
                }),
                SigningRoomId = signingRooms[0].Id,
                DocumentStatusId = documentStatuses[0].Id
            },
            new Signix.Entities.Entities.Document
            {
                Name = "Service_Agreement.pdf",
                Description = "Professional services agreement",
                ClientId = clients[0].Id,
                FileSize = 1048576, // 1MB
                FileType = "application/pdf",
                DocTags = JsonSerializer.Serialize(new
                {
                    document_type = "contract",
                    service_type = "professional",
                    category = "business",
                    contract_value = 50000,
                    duration_months = 12,
                    department = "procurement",
                    renewal_option = true
                }),
                SigningRoomId = signingRooms[1].Id,
                DocumentStatusId = documentStatuses[0].Id
            },
            new Signix.Entities.Entities.Document
            {
                Name = "Employment_Contract.pdf",
                Description = "Employee onboarding contract",
                ClientId = clients[0].Id,
                FileSize = 786432, // 768KB
                FileType = "application/pdf",
                DocTags = JsonSerializer.Serialize(new
                {
                    document_type = "employment",
                    category = "hr",
                    position = "Software Engineer",
                    salary_range = "75000-95000",
                    start_date = "2024-02-01",
                    department = "engineering",
                    probation_period = 3
                }),
                SigningRoomId = signingRooms[0].Id,
                DocumentStatusId = documentStatuses[0].Id
            }
        };

        context.Documents.AddRange(documents);
        await context.SaveChangesAsync();

        logger.LogInformation("✅ Database seeded successfully!");
        logger.LogInformation($"👥 Created {users.Length} users");
        logger.LogInformation($"🏢 Created {clients.Length} clients");
        logger.LogInformation($"📝 Created {signingRooms.Length} signing rooms");
        logger.LogInformation($"📄 Created {documents.Length} documents");
        logger.LogInformation($"📊 Created {documentStatuses.Length} document statuses");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error seeding database: {Message}", ex.Message);
    }
}
