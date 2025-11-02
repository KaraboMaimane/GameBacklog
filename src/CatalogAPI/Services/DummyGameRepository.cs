using CatalogAPI.Domain;
using CatalogAPI.Interfaces;

namespace CatalogAPI.Services
{
    public class DummyGameRepository : IGameRepository
    {
        private static readonly List<Game> _games = new List<Game>
        {
            // Seed data to test the API immediately
            new Game {Id = Guid.NewGuid(), Title = "Witcher 3: Wild Hunt", Platform = GamePlatform.PC, Status = GameStatus.Dropped},
            new Game {Id = Guid.NewGuid(), Title = "Cyberpunk 2077", Platform = GamePlatform.PS5, Status = GameStatus.Completed},
            new Game { Id = Guid.NewGuid(), Title = "Breath of the Wild", Platform = GamePlatform.Switch, Status = GameStatus.Completed }
        };

        public async Task<IEnumerable<Game>> GetAllGamesAsync()
        {
            // Always use Task.FromResult for async methods that don't await I/O operations
            return await Task.FromResult(_games);
        }

        public async Task<Game?> GetGameByIdAsync(Guid id)
        {
            return await Task.FromResult(_games.FirstOrDefault(g => g.Id == id));
        }

        // --- CRUD Implementation is skipped for now to focus on DI and GET operations ---
        // We will implement all CRUD when we move to the Controller. For now, we only need GET.
        public Task AddGameAsync(Game game)
        {
            // WHY: In a real database, the ID would be set by the DB (e.g., auto-increment)
            //      or by the application logic *before* insertion. Since this is a dummy
            //      repo, we simulate this by creating a new Guid here.
            //      This ensures the object in our list has a unique ID.
            if (game.Id == Guid.Empty)
            {
                game.Id = Guid.NewGuid();
            }
            _games.Add(game);
            
            // WHY: Task.CompletedTask is used for async methods that don't have
            //      any real I/O to await, fulfilling the async contract without blocking.
            return Task.CompletedTask;
        }

        public Task<bool> UpdateGameAsync(Game game)
        {
            var existingGame = _games.FirstOrDefault(g => g.Id == game.Id);
            if (existingGame == null)
            {
                // CRITICAL: Returns false if not found, allowing the controller to return 404.
                return Task.FromResult(false);
            }

            existingGame.Title = game.Title;
            existingGame.Platform = game.Platform;
            existingGame.Status = game.Status;
            return Task.FromResult(true);
        }

          public Task<bool> DeleteGameAsync(Guid id)
        {
            // List.RemoveAll returns the count of items removed.
            var rowsAffected = _games.RemoveAll(g => g.Id == id);
            // CRITICAL: Returns true if rows were affected, false otherwise (allowing controller 404).
            return Task.FromResult(rowsAffected > 0);
        }
    }
}