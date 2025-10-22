using System;
using CatalogAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Persistence;

/// <summary>
/// The Entity Framework Core database context for the Catalog API.
///
/// WHY: This class is EF Core's "session" with the database. It manages
/// the connection, transaction, and mapping of our C# objects (Entities)
/// to database tables. We define a DbSet<Game> to tell EF Core
/// that we want to persist Game objects in a table called "Games".
///
/// LEARNING GOAL: Understand the role of the DbContext. It's the bridge
/// between our C# domain models and the actual database. We configure it
/// (e.g., UseInMemoryDatabase) in Program.cs.
/// </summary>
public class GameDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// Gets or sets the DbSet for Games. This represents the "Games" table
    /// in our database.
    /// </summary>
    public DbSet<Game> Games { get; set; }
}
