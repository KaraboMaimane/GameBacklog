// WHY: This file contains Unit Tests for the GamesController. Unit Tests are essential for
//      verifying the controller's behavior (handling HTTP requests, calling the repository)
//      in isolation, without hitting a real database.
// TRADE-OFFS (PRO): Fast execution, high reliability (no external dependencies), excellent
//      for preventing regressions (bugs reappearing).
// TRADE-OFFS (CON): Requires extra setup time (using Moq) and doesn't test the entire
//      system pipeline (that's for Integration Tests, coming later).
// ALTERNATIVES CONSIDERED: Testing with the real DummyGameRepository. Rejected because it
//      makes the test dependent on the implementation, not the interface, which violates the
//      principle of unit testing (isolation).
// LEARNING GOAL: Master the Arrange-Act-Assert (AAA) pattern and the use of Moq to isolate dependencies.

//Quick heads up. I have never in my coding career had to do any unit testing and Im very wet behid the ears. Some of these concepts take a while to stick.
using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CatalogAPI.Domain;
using CatalogAPI.Interfaces;
using CatalogAPI.Controllers;
using System.Linq;
using CatalogAPI.Dtos;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;

namespace CatalogAPI.Tests;

public class GameControllerTests
{
    // 1. Arrange: Define the dependencies needed for the test class
    private readonly Mock<IGameRepository> _mockRepo;
    private readonly GamesController _controller;

    public GameControllerTests()
    {
        // WHY: Initialize Moq to create a mock version of the IGameRepository interface.
        //      The controller only knows about the IGameRepository contract, so we tell
        //      Moq exactly what data to return when the controller calls its methods.
        _mockRepo = new Mock<IGameRepository>();

        // WHY: Instantiate the controller, injecting the mock object instead of the real one.
        //      This isolates the controller logic from the repository implementation.
        _controller = new GamesController(_mockRepo.Object);
    }

    // --- Test Case 1: Happy Path (Data Exists) ---
    [Fact] // Attribute that lets us know this method is a test case
    public async Task GetAllGames_ReturnsOkWithListOfGames()
    {
        // ARRANGE: Set up the mock to return a known list of games.
        var expectedGames = new List<Game>
        {
            new Game { Id = Guid.NewGuid(), Title = "Mock Game 1", Status = GameStatus.Playing},
            new Game { Id = Guid.NewGuid(), Title = "Mock Game 2", Status = GameStatus.Planned},
        };

        //Moq Setup: When the controlle calls GetAllGamesAsync(), return the expected list.
        _mockRepo.Setup(repo => repo.GetAllGamesAsync())
            .ReturnsAsync(expectedGames);

        // ACT: Call the method that we are testing in this scenario GetAllGames()
        var result = await _controller.GetAllGames();

        // ASSERT: Verify the outcome.

        // 1. Verify the HTTP response type is OK (200).
        Assert.IsType<OkObjectResult>(result.Result);

        // 2. Extract the actual list of games from the OkObjectResult.
        var okResult = result.Result as OkObjectResult; // I would understand that this returns the outcome or what the status code represents?
        var actualGames = okResult?.Value as IEnumerable<Game>; // Here we take what the actual value being represented is?

        // 3. Verify the number of items and the content match the expected data.
        Assert.Equal(expectedGames.Count, actualGames?.Count());
        Assert.Equal(expectedGames.First().Title, actualGames?.First().Title);
    }

    // --- Test Case 2: Edge Case (Empty List) ---
    [Fact]
    public async Task GetAllGames_ReturnsOkWithEmptyListWhenNoGamesExist()
    {
        var expectedGames = new List<Game>();

        _mockRepo.Setup(repo => repo.GetAllGamesAsync())
            .ReturnsAsync(expectedGames);

        var result = await _controller.GetAllGames();

        Assert.IsType<OkObjectResult>(result.Result);

        var okResult = result.Result as OkObjectResult;
        var actualGames = okResult?.Value as IEnumerable<Game>;

        Assert.Empty(actualGames!);
    }

    [Fact]
    public async Task GetGameById_ReturnsOkWithCorrectGame()
    {
        // ARRANGE: Set up the scenario
        var expectedGame = new Game { Id = Guid.NewGuid(), Title = "ID Test Game", Status = GameStatus.Playing };

        // Moq Setup: When the controller asks for this specific ID, return the expected game.
        _mockRepo.Setup(repo => repo.GetGameByIdAsync(expectedGame.Id))
            .ReturnsAsync(expectedGame);

        // ACT: Call the method with the expected ID
        var result = await _controller.GetGameById(expectedGame.Id);

        // ASSERT: Verify the outcome is 200 OK and the contet is correct. 
        Assert.IsType<OkObjectResult>(result.Result);

        var okResult = result.Result as OkObjectResult;
        var actualGame = okResult?.Value as Game;

        //Verify the core logic: the ID of the returned game matches the requested ID
        Assert.Equal(expectedGame.Id, actualGame?.Id);

        // Verify the mock was called exactly once with the correct ID.
        _mockRepo.Verify(repo => repo.GetGameByIdAsync(expectedGame.Id), Times.Once());
    }

    [Fact]
    public async Task GetGameById_ReturnsNotFound_WhenIdDoesNotExist()
    {
        // ARRANGE: Set up the scenario
        var nonExistentId = Guid.NewGuid();

        //Moq Setup: When the controller asks for ANY GUID, return null (Game not found)
        // We use It.IsAny<Guid>() to ensure that the mock sorks for any ID that we pass
        _mockRepo.Setup(repo => repo.GetGameByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Game?)null); // Crucially, we mock the return of a null Game object

        // ACT: Call tthe method with the non-existent ID
        var result = await _controller.GetGameById(nonExistentId);

        // ASSERT: Verify the outcome is 404 Not Found.
        // WHY: The controller's job is to translate the repository's 'null' into an HTTP 404
        Assert.IsType<NotFoundResult>(result.Result);

        // Ensure no game object was returned in the result value (it shouldn't even be checked).
        Assert.Null(result.Value);
    }

    // --- Test Case 5: Happy Path (Resource Created) ---
    [Fact]
    public async Task CreatedGame_returnCreated_WithCorrrectLocationHeader()
    {
        // Arrange: Set up the scenario
        var newGameDto = new CreateGameDto
        {
            Title = "New Test Game",
            Platform = GamePlatform.PC,
            Status = GameStatus.Planned
        };

        // We need to create a specific Guid to mock the return data and verify the Location header.
        var createdGameId = Guid.NewGuid();

        // Moq Setup 1: Define what the AddGameAsync call should do.
        //Since AddGameAsync is 'fire and forget' (return Task), we dont need a Setup for the return value.


        // Moq Setup 2: We must mock the repository to ensure the domain model recieves and ID
        // after the AddGameAsync call, which is necessary fo the CreatedAtAciont location header.
        // We use Callback() to modify the 'game' object passed to the repository.
        _mockRepo.Setup(repo => repo.AddGameAsync(It.IsAny<Game>()))
            .Callback<Game>(game => game.Id = createdGameId)
            .Returns(Task.CompletedTask);

        // ACT: Call the method wit hthe valid DTO.
        var result = await _controller.CreateGame(newGameDto);

        // ASSERT: Verify the outcome.

        // 1. Verify the HTTP response type is 201 Created.
        Assert.IsType<CreatedAtActionResult>(result.Result);

        var createdResult = result.Result as CreatedAtActionResult;

        // 2. Verify the location header poits to the correct action and ID.
        Assert.Equal(nameof(GamesController.GetGameById), createdResult?.ActionName);
        Assert.Equal(createdGameId, (createdResult?.RouteValues?["id"]));

        // 3. Verify the mock AddGameAsync was called exactly once.
        _mockRepo.Verify(repo => repo.AddGameAsync(It.IsAny<Game>()), Times.Once());
    }

    [Fact]
    public async Task CreateGame_ReturnsBadRequest_WhenModelIsInvalid()
    {
        // ARRANGE: Set up the scenario
        var invalidDto = new CreateGameDto
        {
            Title = new string('A', 101), // Fails [StringLength(100)] validation
            Platform = GamePlatform.PC,
            Status = GameStatus.Planned
        };

        // CRITICAL STEP: Manually fore the controller's ModelState to be invalid
        // WHY: In production, the framework does this automatically. In a unit test,
        //      we must simulate the failure for the [ApiController] attribute to work.
        _controller.ModelState.AddModelError("Title", "Title must not exceed 100 characters");

        // ACT: Call the method with the invalid DTO.
        var result = await _controller.CreateGame(invalidDto);

        // ASSERT: Verify the outcome is 400 Bad Request.
        // Use a cast to ControllerBase to check the status code directly
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        // We verify the status code of the result is 400
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        // Verify the repository was NEVER called
        _mockRepo.Verify(repo => repo.AddGameAsync(It.IsAny<Game>()), Times.Never());
    }

    // --- Test Case 7: PUT Happy Path ---
    [Fact]
    public async Task UpdateGame_ReturnsOkWithUpdatedGame()
    {
        // ARRANGE
        var existingId = Guid.NewGuid();
        var updateDto = new CreateGameDto { Title = "Updated Title", Platform = GamePlatform.XboxSeries, Status = GameStatus.Playing };

        // Moq Setup: Simulate a successful update (UpdatedGameAsync returns true)
        _mockRepo.Setup(repo => repo.UpdateGameAsync(It.Is<Game>(g => g.Id == existingId))).ReturnsAsync(true);

        // ACT
        var result = await _controller.UpdateGame(existingId, updateDto);

        // ASSERT
        // 1. Verify response is 200 OK.
        Assert.IsType<OkObjectResult>(result.Result);

        var okResult = result.Result as OkObjectResult;
        var updatedGame = Assert.IsType<Game>(okResult?.Value);

        // 2. Verify the model returned contains the new updated title.
        Assert.Equal(updateDto.Title, updatedGame.Title);

        // 3. Verify the correct updated method was called once.
        _mockRepo.Verify(repo => repo.UpdateGameAsync(It.IsAny<Game>()), Times.Once());
    }

    // --- Test Case 8: PUT Failure Path (404) ---
    [Fact]
    public async Task UpdateGame_ReturnsNotFound_WhenIdDoesNotExist()
    {
        // ARRANGE
        var nonExistentId = Guid.NewGuid();
        var updateDto = new CreateGameDto { Title = "Ghost Game", Platform = GamePlatform.PC, Status = GameStatus.Playing };

        // Moq Setup: Simulate failure to find the resource (UpdateGameAsync return false).
        _mockRepo.Setup(repo => repo.UpdateGameAsync(It.IsAny<Game>())).ReturnsAsync(false);

        // ACT
        var result = await _controller.UpdateGame(nonExistentId, updateDto);

        // ASSERT
        // 1. Verify response is 404 Not found.
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- Test Case 9: DELETE Happy Path ---
    [Fact]
    public async Task DeleteGame_ReturnsNoContent_OnSuccess()
    {
        // ARRANGE
        var idToDelete = Guid.NewGuid();

        // Moq Setup: Simulate successfull deletion (DeleteGameAsync returns true)
        _mockRepo.Setup(repo => repo.DeleteGameAsync(idToDelete)).ReturnsAsync(true);

        // ACT
        var result = await _controller.DeleteGame(idToDelete);

        // ASSERT
        // 1. Verify response is 204 No content.
        Assert.IsType<NoContentResult>(result);

        // 2. Verify the correct delete method was called once
        _mockRepo.Verify(repo => repo.DeleteGameAsync(idToDelete), Times.Once());
    }

    // --- Test Case 10: DELETE Failure Path (404) ---
    [Fact]
    public async Task DeleteGame_ReturnsNotFound_WhenIdDoesNotExist()
    {
        // ARRANGE
        var nonExistentId = Guid.NewGuid();

        // Moq Setup: Simulate failure to find the resource (DeleteGameAsync return false)
        _mockRepo.Setup(repo => repo.DeleteGameAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        // ACT
        var result = await _controller.DeleteGame(nonExistentId);

        // ASSERT
        // 1. Verify response is 404 not found.
        Assert.IsType<NotFoundResult>(result);
    }
}
