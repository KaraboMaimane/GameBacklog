using System;
using CatalogAPI.Domain;
using CatalogAPI.Interfaces;
using CatalogAPI.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Services;

public class EfCoreGameRepository : IGameRepository
{
    private readonly GameDbContext _context;

    // WHY: Constructor injection receives the DbContext, which is responsible for the database session.
    public EfCoreGameRepository(GameDbContext context)
    {
        _context = context;
    }

    // Implementation of IGameRepository methods:
    public async Task<IEnumerable<Game>> GetAllGamesAsync()
    {
        // WHY: Uses the DbSet to query all games from the database. ToListAsync() is the
        //      materialization call that executes the query against the database.
        return await _context.Games.ToListAsync();
    }

    public async Task<Game?> GetGameByIdAsync(Guid id)
    {
        // WHY: Uses FindAsync() which is optimized for retrieving entities by their primary key (Id).
        //      It first checks the DbContext tracking cache before hitting the database.
        return await _context.Games.FindAsync(id);
    }

    public async Task AddGameAsync(Game game)
    {
        // WHY: Tells the DbContext to start tracking the new entity.
        _context.Games.Add(game);

        // CRITICAL: SaveChangesAsync executes the SQL INSERT command against the database
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateGameAsync(Game game)
    {
        // First, check if the entity exists.
        var existingGame = await _context.Games.FindAsync(game.Id);

        if (existingGame == null)
        {
            return false;
        }

        //Update teh tracked entity's properties with the new values.
        _context.Entry(existingGame).CurrentValues.SetValues(game);

        // CRITICAL: SaveChangesAsync executes the SQL UPDATE command.
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteGameAsync(Guid id)
    {
        // Find the entity to delete
        var gameToDelete = await _context.Games.FindAsync(id);

        if (gameToDelete == null)
        {
            return false;
        }

        // Tell DbContext to stop tracking and mark for deletion.
        _context.Games.Remove(gameToDelete);

        // CRITICAL: SaveChangesAsync executes the SQL DELETE command
        var rowsAffected = await _context.SaveChangesAsync();

        return rowsAffected > 0;
    }
}
