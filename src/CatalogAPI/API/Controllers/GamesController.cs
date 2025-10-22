using CatalogAPI.Application.Contracts;
using CatalogAPI.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.API.Controllers;

/// <summary>
/// API Controller for managing the game backlog.
///
/// WHY [ApiController]: This attribute enables standard API behaviors
/// like automatic 400 Bad Request responses on invalid model state
/// and inference of request body binding. It's essential.
///
/// WHY [Route("api/[controller]")]: This sets the base route for all
/// actions in this controller to "api/Games". The "[controller]"
/// token is automatically replaced with the controller's name ("Games").
/// This is the standard RESTful routing convention.
///
/// LEARNING GOAL: Understand how ASP.NET Core uses attributes for
/// routing, behavior, and request/response handling.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameRepository _repository;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GamesController"/> class.
    ///
    /// WHY (Constructor Injection): We are injecting the *interface*
    /// IGameRepository, not the concrete implementation. This is
    /// Dependency Inversion. The .NET DI container (configured in
    /// Program.cs) will provide the *concrete* GameRepository at runtime.
    /// </summary>
    /// <param name="repository">The game repository contract.</param>
    public GamesController(IGameRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Game>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Game>>> GetAllGames()
    {
        // 1. Call the repository to get the data.
        var games = await _repository.GetAllAsync();

        // 2. Wrap the data in an Ok() response.
        // WHY: This returns an HTTP 200 OK status code along
        // with the list of games serialized as JSON in the body.
        return Ok(games);
    }


    /// <summary>
    /// Gets a specific game by its ID.
    /// </summary>
    /// <param name="id">The ID of the game to retrieve.</param>
    /// <returns>The requested game.</returns>
    [HttpGet("{id:int}", Name = "GetGameById")]
    [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Game>> GetGameById(int id)
    {
        var game = await _repository.GetByIdAsync(id);

        if (game == null)
        {
            return NotFound();
        }

        return Ok(game);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Game), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Game>> CreateGame([FromBody] Game game)
    {
        // NOTE: In a future milestone, we will add Validation
        // and DTOs. For now, we pass the domain model directly.
        var newGame = await _repository.AddAsync(game);

        // WHY CreatedAtRoute: This is the standard REST-compliant
        // response for a successful POST. It returns an HTTP 201 Created
        // status, a 'Location' header with the URL to the new resource
        // (e.g., /api/Games/5), and the new resource itself in the body.
        return CreatedAtRoute(
            routeName: "GetGameById",
            routeValues: new { id = newGame.Id }, 
            value: newGame);
    }

    /// <summary>
    /// Updates an existing game.
    /// </summary>
    /// <param name="id">The ID of the game to update.</param>
    /// <param name="gameToUpdate">The game data to update.</param>
    /// <returns>No content.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGame(int id, Game gameToUpdate)
    {
        if (id != gameToUpdate.Id)
        {
            return BadRequest("ID mismatch between URL and request body.");
        }

        var existingGame = await _repository.GetByIdAsync(id);
        if (existingGame == null)
        {
            return NotFound();
        }

        await _repository.UpdateAsync(gameToUpdate);
        
        // WHY NoContent: A successful PUT (update) should return
        // an HTTP 204 No Content response. We're confirming the
        // update worked, but we don't need to send the object back.
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGame(int id)
    {
        var gameToDelete = await _repository.GetByIdAsync(id);
        if (gameToDelete == null) return NotFound();
        
        // Pass the *entity* to be deleted. No second query.
        await _repository.DeleteAsync(gameToDelete);
        
        // WHY NoContent: Like PUT, a successful DELETE returns
        // an HTTP 204 No Content. We're just confirming it's gone.
        return NoContent();
    }
}