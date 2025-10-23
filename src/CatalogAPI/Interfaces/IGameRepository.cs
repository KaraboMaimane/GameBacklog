// WHY: Defines a contract for data access operations. This adheres to the Dependency Inversion
//      Principle (DIP) from SOLID: High-level modules (like the Controller) should not depend
//      on low-level modules (like the EF Core implementation); both should depend on abstractions (the Interface).
// TRADE-OFFS (PRO): Decoupling. Allows us to swap the database technology (EF Core, Dapper, NoSQL)
//      without changing the Controller or Service logic. Essential for testing (mocking).
// TRADE-OFFS (CON): Boilerplate. Introduces more files and interfaces than a simple CRUD setup,
//      but the long-term benefits in maintainability and testability are worth the overhead.
// ALTERNATIVES CONSIDERED: None, the Repository pattern with an interface is mandatory for testable,
//      production-grade architecture. Directly calling DbContext from the Controller is an anti-pattern.
// LEARNING GOAL: Understand the 'I' in SOLID (Interface Segregation) and the 'D' (Dependency Inversion).
using CatalogAPI.Domain;

namespace CatalogAPI.Interfaces
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetAllGamesAsync();
        Task<Game?> GetGameByIdAsync(Guid id);
        Task AddGameAsync(Game game);
        Task<bool> UpdateGameAsync(Game game);
        Task<bool> DeleteGameAsync(Guid id);
    }
}