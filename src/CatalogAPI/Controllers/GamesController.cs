// WHY: Controllers serve as the "entry point" for external HTTP requests (the C in MVC).
//      This controller manages all CRUD operations related to the Game entity. It is intentionally
//      kept thin, relying on the injected IGameRepository to handle business logic and data access.
// TRADE-OFFS (PRO): Separation of Concerns (SoC). Keeps the HTTP layer separate from data logic,
//      making the service testable and maintainable.
// TRADE-OFFS (CON): Boilerplate. Requires defining repository interfaces and implementing DI,
//      which adds overhead compared to a single monolithic class.
// ALTERNATIVES CONSIDERED: Using a dedicated Service/Business Logic Layer between the Controller
//      and the Repository. Rejected for now to maintain simplicity, as the Repository currently acts
//      as the business layer. Will be added later for more complex logic.
// LEARNING GOAL: Master Constructor Injection and adhere to the Dependency Inversion Principle (DIP)
//      by injecting the abstraction (interface) and not the concrete implementation.
using CatalogAPI.Domain;
using CatalogAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private IGameRepository _repository;

        // WHY: Constructor Injection. The ASP.NET Core DI container finds the registered service
        //      (IGameRepository, which we set to DummyGameRepository in Program.cs) and passes it
        //      to the controller at runtime. This adheres to DIP.
        // TRADE-OFFS (PRO): Decoupling, Testability (we can easily mock IGameRepository for unit tests).
        // TRADE-OFFS (CON): Requires all dependencies to be correctly registered in Program.cs.
        // ALTERNATIVES CONSIDERED: Service Locator pattern or manually instantiating the repository
        //      ('new DummyGameRepository()'). Both rejected as they violate DIP and make testing impossible.
        public GamesController(IGameRepository repository)
        {
            _repository = repository;
        }
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Game>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Game>>> GetAllGames()
        {
            // WHY: ActionResult<T> wraps the result type (IEnumerable<Game>) and provides
            //      access to HTTP protocol features (status codes, headers). This is the standard
            //      ASP.NET Core convention for Web API methods, allowing us to return 'Ok(data)'
            //      or 'NotFound()' easily.
            // TRADE-OFFS (PRO): Clear separation of concerns between HTTP response and data. Enables
            //      automatic Swagger documentation of possible response types.
            // TRADE-OFFS (CON): Slight verbosity compared to just returning the type T directly.
            // ALTERNATIVES CONSIDERED: Returning just 'IEnumerable<Game>'. Rejected because it removes
            //      the ability to return non-200 status codes (like 404 or 500) with proper formatting.
            // LEARNING GOAL: Master the use of ActionResult<T> and its role in Web API response negotiation.
            var gamesList = await _repository.GetAllGamesAsync();
            return Ok(gamesList);
        }
        /// <summary>
        /// Retrieves a specific game by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the game.</param>
        /// <returns>The requested Game object with 200 OK, or 404 Not Found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Game>> GetGameById(Guid id)
        {
            // WHY: ActionResult<Game> is used here because we might return a 200 (Game object)
            //      or a 404 (no object), which is standard RESTful API design.
            // TRADE-OFFS (PRO): Clear API contract (200 for success, 404 for resource not found).
            // TRADE-OFFS (CON): Requires two branches in the code (check for null).
            // ALTERNATIVES CONSIDERED: Throwing an exception and letting middleware handle it. Rejected
            //      for simple resource-not-found as explicitly returning 404 is cleaner and expected.
            // LEARNING GOAL: Master conditional status code returns (200 vs 404).
            var game = await _repository.GetGameByIdAsync(id);

            if (game == null)
            {
                // WHY: Returning NotFound() immediately signals to the client that the requested
                //      resource does not exist, which is HTTP Status Code 404. This is REST standard.
                return NotFound();
            }

            // Returns an Http 200 OK response with the game object
            return Ok(game);
        }
    }
}
