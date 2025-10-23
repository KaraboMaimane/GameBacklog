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

    // Key Notes:
    // You sort of explained why these unit tests are implemented. Maybe I need to rest my brain for today because Im lost with the thought process as to the how we do this and why 
    // Im still very confused by these unit testing terms like your asserts and how we know if empty or is type etc. 
    // Also the naming convetions for the test cases. I also get confused as to how would I know what to test against or what test cases to create. 
}
