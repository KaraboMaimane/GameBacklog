// WHY: DbContext is the central class in Entity Framework Core responsible for session
//      with the database. It manages connections, transactions, and most importantly,
//      the mapping of our C# domain objects (entities) to the database tables.
// TRADE-OFFS (PRO): Automatic tracking of changes (saving boilerplate), integrated Unit of Work pattern.
// TRADE-OFFS (CON): "Magic" behavior (sometimes hard to debug), can lead to performance issues if LINQ queries are complex.
// ALTERNATIVES CONSIDERED: Dapper (a micro-ORM). Rejected because EF Core is better suited for handling complex domain relationships and is the standard starting point for .NET microservices.
// LEARNING GOAL: Understand DbContext's role as the ORM and Unit of Work.
using System;
using CatalogAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Persistence;

public class GameDbContext : DbContext
{
    // WHY: The base constructor is required to configure the database provider 
    //      (in our case, PostgreSQL) and the connection string, which happens in Program.cs.
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    // WHY: This DbSet represents the collection of all Game entities that can be queried 
    //      from the database. EF Core will automatically map this to a table named "Games".
    public DbSet<Game> Games { get; set; } = default!;

    // WHY: OnModelCreating is a perfect place to enforce constraints or define relationships 
    //      that can't be handled by simple C# attributes (like [Required]).
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Example of a configuration done here:
        // Ensures the Title property cannot be null in the database.
        modelBuilder.Entity<Game>()
            .Property(g => g.Title)
            .IsRequired();
        
        // Note: EF Core will automatically convert our GamePlatform and GameStatus enums
        //       into integer columns in the database by default.
    }
}
