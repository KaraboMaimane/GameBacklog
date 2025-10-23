using CatalogAPI.Application.Contracts;
using CatalogAPI.Domain;
using CatalogAPI.Persistence;
using CatalogAPI.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Tests;

public class GameRepositoryTests
{
    /// <summary>
    /// Unit tests for the GameRepository.
    ///
    /// WHY: We must test our data access logic. Since this is a unit test,
    /// we don't want to hit a real database. Instead, we mock the
    /// dependencies.
    ///
    /// HOW: We use the *In-Memory* database provider as a "test double."
    /// We get a fresh, empty in-memory database for *each test*.
    /// This gives us isolation without the complexity of mocking
    /// DbContext and DbSet, which is notoriously difficult and brittle.
    ///
    /// This is a hybrid approach: it's not a *pure* unit test (it tests
    /// EF Core's in-memory provider), but it's not an integration test
    /// (it doesn't hit a real SQL server). It's the most pragmatic
    /// way to test EF Core-dependent repositories.
    ///
    /// LEARNING GOAL: Learn how to effectively test EF Core repositories
    /// using a clean in-memory database instance for each test.
    /// </summary>

    private readonly GameDbContext _context;
    private readonly IGameRepository _repository;

    public GameRepositoryTests()
    {
        // 1. Arrange (Setup)
        // We create new DbContext options for an in-memory database.
        // We give it a unique database name (a new GUID) for *every test*
        // to ensure 100% test isolation.
        var dbOptions = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new GameDbContext(dbOptions);
        _repository = new GameRepository(_context);
    }

    [Fact]
    public async Task AddAsync_WithValidGame_ShouldAddGameToContext()
    {
        // 2. Arrange (Test-specific)
        var newGame = new Game
        {
            Title = "Elden Ring",
            Developer = "FromSoftware",
            Genre = "Action RPG",
            DateAdded = DateTime.UtcNow
        };

        // 3. Act
        var addedGame = await _repository.AddAsync(newGame);
        
        // 4. Assert
        // We use two assertions:
        // First, check the object returned by the method.
        addedGame.Id.Should().NotBe(0);
        addedGame.Title.Should().Be("Elden Ring");
        
        // Second, check the database context *directly* to be sure
        // the data was actually saved.
        var gameInDb = await _context.Games.FindAsync(addedGame.Id);
        gameInDb.Should().NotBeNull();
        gameInDb.Should().BeEquivalentTo(addedGame);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnGame()
    {
        // arrange
        var game = new Game
        {
            Title = "Cyberpunk 2077",
            Developer = "CD Projekt Red",
            Genre = "Action RPG",
            DateAdded = DateTime.UtcNow
        };
        await _repository.AddAsync(game); // add it first to db
        
        //Act
        var foundGame = await _repository.GetByIdAsync(game.Id);

        foundGame.Should().NotBeNull();
        foundGame.Should().BeEquivalentTo(game);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        // (Database is empty)
        
        //Act
        var foundGame = await _repository.GetByIdAsync(999);

        //Assert
        foundGame.Should().BeNull();
    }
    
    [Fact]
    public async Task DeleteAsync_WithExistingGame_ShouldRemoveGameFromContext()
    {
        // Arrange
        var game = new Game
        {
            Title = "Stray",
            Genre = "Adventure",
            DateAdded = DateTime.UtcNow
        };
        await _repository.AddAsync(game);

        // Sanity check: ensure it's in the context
        var gameInDb = await _context.Games.FindAsync(game.Id);
        gameInDb.Should().NotBeNull();

        // Act
        // We pass the *entity* we just added
        await _repository.DeleteAsync(game);

        // Assert
        // Check the context directly to see if it's gone
        var deleteGameInDb = await _context.Games.FindAsync(game.Id);
        deleteGameInDb.Should().BeNull();
    }
}