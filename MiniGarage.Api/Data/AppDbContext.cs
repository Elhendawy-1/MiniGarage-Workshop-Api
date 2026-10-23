using Microsoft.EntityFrameworkCore;
using MiniGarage.Api.Models;

namespace MiniGarage.Api.Data;

/// <summary>
/// AppDbContext is the central EF Core class that represents a session with the database.
/// 
/// How it works:
/// 1. AppDbContext inherits from DbContext (provided by EF Core).
/// 2. We declare a DbSet&lt;Car&gt; — this tells EF Core: "I want a table for Car entities."
/// 3. EF Core reads our model, discovers the Car class and its properties,
///    and maps them to a SQL Server table called "Cars".
/// 4. When we call SaveChangesAsync(), EF Core inspects the change tracker,
///    generates the appropriate INSERT / UPDATE / DELETE SQL, and executes it.
/// 
/// Dependency Injection:
/// AppDbContext is registered in Program.cs via AddDbContext&lt;AppDbContext&gt;().
/// ASP.NET Core injects it into the controller constructor automatically.
/// By default, DbContext has a Scoped lifetime — one instance per HTTP request.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Constructor receives DbContextOptions injected by the DI container.
    /// DbContextOptions carries the connection string and provider (SQL Server).
    /// We pass it to the base DbContext constructor.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet&lt;Car&gt; represents the "Cars" table.
    /// Through this property we write LINQ queries:
    ///   await _db.Cars.ToListAsync()       → SELECT * FROM Cars
    ///   await _db.Cars.FindAsync(id)        → SELECT * FROM Cars WHERE Id = @id
    ///   _db.Cars.Add(car)                   → marks entity as Added (not yet saved)
    ///   _db.Cars.Remove(car)                → marks entity as Deleted (not yet saved)
    ///   await _db.SaveChangesAsync()        → executes the actual SQL
    /// </summary>
    public DbSet<Car> Cars { get; set; }

    /// <summary>
    /// OnModelCreating is called once when the model is first built.
    /// Here we can fine-tune column types, indexes, constraints, and seed data.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Car>(entity =>
        {
            // Table name (optional — EF Core would default to "Cars" anyway by convention)
            entity.ToTable("Cars");

            // Primary key (also by convention, but explicit is clearer for learning)
            entity.HasKey(c => c.Id);

            // Column type constraints — EF Core sends these to SQL Server during migration
            entity.Property(c => c.OwnerName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.CarModel)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.LicensePlate)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(c => c.ProblemDescription)
                  .IsRequired()
                  .HasMaxLength(500);

            // Enum stored as int (default). Use .HasConversion<string>() to store as text.
            entity.Property(c => c.RepairStatus)
                  .IsRequired();

            entity.Property(c => c.CreatedAt)
                  .IsRequired();

            // Unique index on LicensePlate — no two cars share the same plate
            entity.HasIndex(c => c.LicensePlate)
                  .IsUnique()
                  .HasDatabaseName("IX_Cars_LicensePlate");

            // Non-clustered index on CreatedAt to support OrderByDescending(CreatedAt) without table scans
            entity.HasIndex(c => c.CreatedAt)
                  .HasDatabaseName("IX_Cars_CreatedAt");
        });

        // Seed data — EF Core will insert these rows during the initial migration
        modelBuilder.Entity<Car>().HasData(
            new Car
            {
                Id = 1,
                OwnerName = "Ahmed Al-Rashidi",
                CarModel = "Toyota Corolla",
                LicensePlate = "ABC-1234",
                ProblemDescription = "Engine overheating",
                RepairStatus = RepairStatus.InProgress,
                CreatedAt = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc)
            },
            new Car
            {
                Id = 2,
                OwnerName = "Sara Al-Mutairi",
                CarModel = "Honda Civic",
                LicensePlate = "XYZ-5678",
                ProblemDescription = "Brake pads worn out",
                RepairStatus = RepairStatus.Pending,
                CreatedAt = new DateTime(2026, 1, 11, 9, 30, 0, DateTimeKind.Utc)
            }
        );
    }
}
