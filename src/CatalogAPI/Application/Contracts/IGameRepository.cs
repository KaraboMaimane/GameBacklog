using System;
using CatalogAPI.Domain;

namespace CatalogAPI.Application.Contracts;

/// <summary>
/// Interface for the game repository.
///
/// WHY: This is the "contract" for our persistence layer. It defines *what*
/// data operations are possible, but not *how* they are implemented.
/// This is the "I" in SOLID - Interface Segregation. More importantly, it's
/// the "D" - Dependency Inversion.
///
/// Our Application layer (services) will depend on this *abstraction*
/// (IGameRepository), not on a concrete implementation (like GameRepository).
///
/// TRADE-OFFS:
/// - PRO: Decouples business logic from data access. We can swap the
///   implementation from In-Memory to SQL Server, and the services won't change.
/// - PRO: Makes testing easy. We can mock this interface in our unit tests.
/// - CON: One extra file/layer of abstraction. For a trivial
///   "weekend project," this might be overkill, but for a production
///   system, it's non-negotiable.
///
/// ALTERNATIVES CONSIDERED:
/// - Direct DbContext injection into services: This is faster to write but
///   is a terrible practice. It tightly couples your business logic to EF Core.
///   Your services suddenly need to know about DbSets, SaveChangesAsync, etc.
///
/// LEARNING GOAL: Master the Dependency Inversion Principle.
/// Depend on abstractions, not concretions.
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Retrieves a game by its unique ID.
    /// </summary>
    /// <param name="id">The ID of the game.</param>
    /// <returns>The game object, or null if not found.</returns>
    Task<Game?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all games from the backlog.
    /// </summary>
    /// <returns>A read-only list of all games.</returns>
    Task<IReadOnlyList<Game>> GetAllAsync();

    Task<Game> AddAsync(Game game);
    /// <summary>
    /// Updates an existing game in the backlog.
    /// </summary>
    /// <param name="game">The game object to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Game game);
    Task DeleteAsync(Game game);
}
