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
using CatalogAPI.Dtos;
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

        /// <summary>
        /// Creates a new game in the backlog.
        /// </summary>
        /// <param name="createGameDto">The DTO containing data for the new game.</param>
        /// <returns>A 201 Created response with the new game's location, or 400 Bad Request.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Game), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Game>> CreateGame(CreateGameDto createGameDto)
        {
            // WHY: ASP.NET Core automatically performs Model Validation on the 'createGameDto'
            //      because of the [ApiController] attribute on our controller.
            //      If 'Title' is missing (or any [Required] field), the framework *automatically*
            //      stops execution and returns a 400 Bad Request *before* our code runs.
            //      This is a massive win for security and clean code.

            if (!ModelState.IsValid)
            {
                // This is where the framework would have stopped the request and formatted
                // the error details into a BadRequestObjectResult for us. We simulate it
                // by returning the same result type the framework provides.
                return BadRequest(ModelState);
            }

            // 1. MAPPING: Convert the DTO (API contract) to the Domain Model (internal logic)
            // WHY: This is the "boilerplate" cost of using DTOs. We must manually map
            //      the fields. Later, we can use a tool like AutoMapper to automate this.
            // LEARNING GOAL: Understand the mapping step is where we translate external
            //      data into our trusted internal domain model.
            var game = new Game
            {
                // ID is *not* set here. It will be set by the repository/database.
                Title = createGameDto.Title,
                Platform = createGameDto.Platform,
                Status = createGameDto.Status
            };

            // 2. REPOSITORY CALL: Pass the *domain model* to the repository.
            await _repository.AddGameAsync(game);

            // 3. HTTP RESPONSE: Return the standard RESTful response for creation.
            // WHY: A POST that successfully creates a resource *must* return a 201 Created.
            //      CreatedAtAction is the standard helper for this. It generates:
            //      a) An HTTP 201 Status Code.
            //      b) A 'Location' header in the response (e.g., /api/games/{new-guid})
            //      c) The newly created 'game' object in the response body.
            return CreatedAtAction(nameof(GetGameById), new { id = game.Id }, game);
        }

        /// <summary>
        /// Updates an existing game entry.
        /// </summary>
        /// <param name="id">The unique identifier of the game to update.</param>
        /// <param name="updateGameDto">The DTO containing updated data.</param>
        /// <returns>200 OK if successful, 400 Bad Request if invalid input, 404 Not Found if ID does not exist.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Game>> UpdateGame(Guid id, CreateGameDto updateGameDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. MAPPING: Create a full Game model, ensuring we use the ID from the route.
            // WHY: The client sends the ID in the URL, not the body (DTO). We merge them here.
            var gameToUpdate = new Game
            {
                Id = id,
                Title = updateGameDto.Title,
                Platform = updateGameDto.Platform,
                Status = updateGameDto.Status
            };

            // 2. REPOSITORY CALL: Attempt to update the resource.
            var success = await _repository.UpdateGameAsync(gameToUpdate);

            // 3. HTTP RESPONSE: Check the outcome of the repository call.
            // WHY: UpdateGameAsync returns false if the resource was not found. This translates to 404.
            if (!success)
            {
                return NotFound();
            }

            // WHY: A successful PUT operation typically returns 200 OK along with the updated resource.
            return Ok(gameToUpdate);
        }


        /// <summary>
        /// Deletes a specific game entry by ID.
        /// </summary>
        /// <param name="id">The unique identifier of the game to delete.</param>
        /// <returns>204 No Content if successful, 404 Not Found if ID does not exist.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGame(Guid id) // Note: IActionResult is used as we return no content.
        {
            // WHY: No DTO or ModelState check is needed for DELETE, only the ID from the route.

            // 1. REPOSITORY CALL: Attempt to delete the resource.
            var success = await _repository.DeleteGameAsync(id);

            // 2. HTTP Response: Check the outcome
            if (!success)
            {
                // The resource does not exist for us to delete.
                return NotFound();
            }
            
            // WHY: The standard REST response for a successful DELETE is 204 No Content.
            //      We use IActionResult (instead of ActionResult<T>) because there is no object (T) returned.
            return NoContent();
        } 
    }
}
