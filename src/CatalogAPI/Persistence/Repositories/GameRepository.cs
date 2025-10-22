using CatalogAPI.Application.Contracts;
using CatalogAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the IGameRepository.
///
/// WHY: This is the concrete implementation of our repository contract.
/// It uses Entity Framework Core to perform CRUD (Create, Read, Update, Delete)
/// operations against the database. It depends on the GameDbContext, which
/// is injected via the constructor (Dependency Injection).
///
/// This class is the *only* class in our application (so far) that
/// directly "knows" about EF Core. Our services don't know. Our
/// controllers don't know. This isolation is the entire point.
///
/// TRADE-OFFS (Repository Pattern):
/// - PRO: Perfect abstraction. Easy to test, easy to swap.
/// - CON: Adds a layer of indirection. For simple operations,
///   `_context.Games.AddAsync(game)` is very similar to what a service
///   would do. The *real* value comes when queries get complex, or when
///   you need to add caching, logging, etc., at the repository level.
///
/// LEARNING GOAL: Implement a repository pattern and see how it
/// cleanly abstracts data access logic using EF Core.
/// </summary>
public class GameRepository : IGameRepository
{
    private readonly GameDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public GameRepository(GameDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Game> AddAsync(Game game)
    {
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();
        return game;
    }

    public async Task DeleteAsync(int id)
    {
        var gameToDelete = await _context.Games.FindAsync(id);
        if (gameToDelete != null)
        {
            _context.Games.Remove(gameToDelete);
            await _context.SaveChangesAsync();
        }
        // Note: We might want to throw a NotFoundException here
        // if gameToDelete is null. We'll add better error
        // handling in a later milestone.
    }

    public async Task<IReadOnlyList<Game>> GetAllAsync()
    {
        return await _context.Games.AsNoTracking().ToListAsync();
        // WHY AsNoTracking(): This is a read-only query.
        // We tell EF Core not to bother tracking changes for these
        // entities, which is a performance optimization.
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _context.Games.FindAsync(id);
    }

    public async Task UpdateAsync(Game game)
    {
        // Entry(game).State tells EF Core that the 'game' object
        // we have is an *existing* one and it should be marked as 'Modified'.
        _context.Entry(game).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}