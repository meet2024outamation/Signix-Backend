using Microsoft.EntityFrameworkCore;
using Signix.Entities.Entities;
using System.Text.Json;

namespace Signix.Entities.Context;

public class SignixDbContext : DbContext
{
    public SignixDbContext(DbContextOptions<SignixDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Designation> Designations { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentStatus> DocumentStatuses { get; set; }
    public DbSet<Signer> Signers { get; set; }
    public DbSet<SigningRoom> SigningRooms { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Document>(entity =>
        {
            //entity.HasOne(d => d.Client)
            //.WithMany(c => c.Documents)
            //.HasForeignKey(d => d.ClientId)
            //.OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.DocumentStatus)
                  .WithMany(ds => ds.Documents)
                  .HasForeignKey(d => d.DocumentStatusId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.SigningRoom)
                  .WithMany(sr => sr.Documents)
                  .HasForeignKey(d => d.SigningRoomId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.DocTags).HasColumnType("jsonb").HasConversion(v => JsonSerializer.Serialize(v, new JsonSerializerOptions { }),
                                  v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { })!
                                );
        });

        modelBuilder.Entity<SigningRoom>(entity =>
        {
            entity.HasOne(sr => sr.Notary)
                  .WithMany(u => u.SigningRooms)
                  .HasForeignKey(sr => sr.NotaryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Client)
                .WithMany(c => c.SigningRooms)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Signer>(entity =>
        {
            entity.HasOne(s => s.SigningRoom)
                  .WithMany(sr => sr.Signers)
                  .HasForeignKey(s => s.SigningRoomId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint
            entity.HasIndex(s => new { s.SigningRoomId, s.Email }).IsUnique();
        });

        // ✅ PostgreSQL-specific config
        modelBuilder.Entity<Client>(entity =>
        {
            entity.Property(e => e.ClientSecret).HasColumnType("jsonb").HasConversion(v => JsonSerializer.Serialize(v, new JsonSerializerOptions { }),
                                  v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { })!
                                );
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.MetaData).HasColumnType("jsonb").HasConversion(v => JsonSerializer.Serialize(v, new JsonSerializerOptions { }),
                                  v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { })!
                                );
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<SigningRoom>(entity =>
        {
            entity.Property(e => e.MetaData).HasColumnType("jsonb").HasConversion(v => JsonSerializer.Serialize(v, new JsonSerializerOptions { }),
                                  v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { })!
                                );
            entity.Property(e => e.SignTags).HasColumnType("jsonb").HasConversion(v => JsonSerializer.Serialize(v, new JsonSerializerOptions { }),
                                  v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { })!
                                );
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });

        // Add performance indexes
        modelBuilder.Entity<SigningRoom>().HasIndex(d => d.ClientId);
        modelBuilder.Entity<Document>().HasIndex(d => d.SigningRoomId);
        modelBuilder.Entity<Document>().HasIndex(d => d.DocumentStatusId);
        modelBuilder.Entity<SigningRoom>().HasIndex(sr => sr.NotaryId);
        modelBuilder.Entity<Signer>().HasIndex(s => s.SigningRoomId);

        // Seed initial data
        //SeedInitialData(modelBuilder);
    }

    //private void SeedInitialData(ModelBuilder modelBuilder)
    //{
    //    // Seed DocumentStatus
    //    modelBuilder.Entity<DocumentStatus>().HasData(
    //        new DocumentStatus { Id = 1, Name = "Draft", Description = "Document is in draft state", IsActive = true },
    //        new DocumentStatus { Id = 2, Name = "Pending", Description = "Document is pending signature", IsActive = true },
    //        new DocumentStatus { Id = 3, Name = "Signed", Description = "Document has been signed", IsActive = true },
    //        new DocumentStatus { Id = 4, Name = "Rejected", Description = "Document was rejected", IsActive = true },
    //        new DocumentStatus { Id = 5, Name = "Expired", Description = "Document signing has expired", IsActive = true }
    //    );

    //    // Seed Designations
    //    modelBuilder.Entity<Designation>().HasData(
    //        new Designation { Id = 1, Name = "CEO", Description = "Chief Executive Officer", IsActive = true },
    //        new Designation { Id = 2, Name = "CTO", Description = "Chief Technology Officer", IsActive = true },
    //        new Designation { Id = 3, Name = "Manager", Description = "Department Manager", IsActive = true },
    //        new Designation { Id = 4, Name = "Director", Description = "Company Director", IsActive = true },
    //        new Designation { Id = 5, Name = "Employee", Description = "Regular Employee", IsActive = true }
    //    );
    //}

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-update ModifiedAt timestamps
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity.GetType().GetProperty("ModifiedAt") != null &&
                       (e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            entry.Property("ModifiedAt").CurrentValue = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
