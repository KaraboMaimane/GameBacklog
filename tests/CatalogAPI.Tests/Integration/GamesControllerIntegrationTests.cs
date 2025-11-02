using System;
using System.Net;
using System.Net.Http.Json;
using CatalogAPI.Domain;
using CatalogAPI.Dtos;

namespace CatalogAPI.Tests.Integration;

public class GamesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public GamesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        // Create an HttpClient that is pre-configured to talk to our in-memory server
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task PostGame_Then_GetGameById_ReturnSuccessfuly()
    {
        // Arrange
        var newGameDto = new CreateGameDto
        {
            Title = "Integration Test Game",
            Platform = GamePlatform.PC,
            Status = GameStatus.Playing
        };

        // ACT (POST)
        // 1. Send a real HTTP POST request
        var postResponse = await _client.PostAsJsonAsync("/api/games", newGameDto);

        // ASSERT (POST)
        // 1. Verify the HTTP 201 Created status
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        // 2. Extract the newly created game from the POST response body
        var createdGame = await postResponse.Content.ReadFromJsonAsync<Game>();
        Assert.NotNull(createdGame);
        Assert.Equal("Integration Test Game", createdGame.Title);

        var newGameId = createdGame.Id;
        var locationHeader = postResponse.Headers.Location;

        // 3. Verify the Location header is correct

        // WHY: The integration test client sometimes returns a fully qualified URI (http://localhost/...). 
        //      We use a URI object to normalize the path and compare only the absolute URI path 
        //      (e.g., "/api/games/...") to make the test environment-agnostic.
        // CRITICAL FIX: To handle the casing issue (Games vs games), we use ToLowerInvariant() 
        //               on both strings before comparison.

        Assert.NotNull(locationHeader); // First, ensure the header exists

        // Get the absolute path from the URI object (e.g., "/api/Games/{guid}")
        var actualPath = locationHeader.AbsolutePath;

        // Define the expected path using the specific Guid generated
        var expectedPath = $"/api/Games/{newGameId}";

        // IMPORTANT: Compare the strings using invariant culture and explicit casing check.
        Assert.Equal(expectedPath, actualPath, ignoreCase: true);

        // --- Continue with SEPARATE VALIDATION STEP (GET) ---

        // ACT (GET)
        // 4. Send a real HTTP GET request using the actual Location header URI
        var getResponse = await _client.GetAsync(locationHeader); // Use the full Location URI here
    }
}
