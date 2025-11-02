# GameBacklog API

## 1. Introduction: The "Why"

This project is a simple RESTful API for managing a personal video game backlog. While the feature set is straightforward, its primary purpose is to serve as an educational tool and a practical example for developers new to .NET, API development, and common software architecture patterns.

For a junior developer, this codebase is designed to be a clear and approachable demonstration of how to build a web API using modern .NET, following best practices that are highly valued in the industry. It answers questions like:

*   How do you structure a .NET API project cleanly?
*   How do you separate different concerns like API logic, business rules, and data access?
*   How do you write code that is testable and maintainable?
*   What are some foundational patterns used in professional software development?

Think of this project not just as an application, but as a guided tour through the fundamentals of building robust and scalable backend services.

## 2. Core Learning Concepts

This project intentionally uses several important software design patterns and concepts. As you explore the code, focus on understanding the following:

| Concept                 | Where to Look                               | What You'll Learn                                                                                                                              |
| ----------------------- | ------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| **RESTful API Design**  | `src/CatalogAPI/Controllers/GamesController.cs` | How to define API endpoints (like `GET`, `POST`, `PUT`, `DELETE`) and use HTTP verbs and status codes correctly to create a web-standard API.     |
| **Repository Pattern**  | `Interfaces/IGameRepository.cs`, `Services/DummyGameRepository.cs` | A powerful pattern for decoupling your application from the data source. Notice how the controller depends on the `IGameRepository` interface, not the concrete `DummyGameRepository`. This makes it easy to swap out the in-memory data for a real database later without changing the API logic. |
| **Dependency Injection (DI)** | `Program.cs` (look for `builder.Services.AddSingleton...`) | A core feature of ASP.NET Core. See how the `IGameRepository` is "injected" into the `GamesController`. This promotes loose coupling and makes testing much easier. |
| **Domain-Driven Design (DDD) - Basics** | `src/CatalogAPI/Domain/` folder | The `Game.cs`, `GamePlatform.cs`, and `GameStatus.cs` files represent our "Domain". This is the heart of our application's business logic, independent of any framework or database. |
| **Data Transfer Objects (DTOs)** | `src/CatalogAPI/Dtos/` folder | The `CreateGameDto.cs` is used to shape the data sent to our API. This prevents exposing our internal `Game` domain model directly to the outside world, providing a layer of security and flexibility. |
| **Unit Testing**        | `tests/CatalogAPI.Tests/` folder            | See how to write tests for your API controllers. This ensures your code works as expected and prevents regressions when you make changes.         |

## 3. Technology Stack

*   **.NET 8**: The core framework for building the application.
*   **ASP.NET Core**: Used for building the web API.
*   **xUnit**: The testing framework used for our unit tests in the `CatalogAPI.Tests` project.
*   **In-Memory Database**: The `DummyGameRepository` acts as a simple, temporary database that lives in the application's memory.

## 4. Onboarding: Getting Started

Follow these steps to get the project running on your local machine.

### Prerequisites

1.  **.NET 8 SDK**: You must have the .NET 8 SDK installed. You can download it from the [official .NET website](https://dotnet.microsoft.com/download/dotnet/8.0).
2.  **A Code Editor**: Visual Studio 2022, VS Code, or JetBrains Rider.
3.  **(Optional) .NET HTTP REPL Tool**: For easily testing the API endpoints. Install it by running:
    ```bash
    dotnet tool install -g Microsoft.dotnet-httprepl
    ```

### Build and Run the Application

1.  **Clone the repository** (if you haven't already).
2.  **Navigate to the root folder** (`/GameBacklog/`) in your terminal.
3.  **Build the solution** to restore dependencies and compile the code:
    ```bash
    dotnet build
    ```
4.  **Run the API project**:
    ```bash
    dotnet run --project src/CatalogAPI/CatalogAPI.csproj
    ```
    The API will now be running. The terminal will show the URL where it's listening (e.g., `http://localhost:5000`).

### Run the Tests

It's crucial to run the tests to ensure everything is working as expected.

1.  **Navigate to the root folder** in your terminal.
2.  **Execute the test suite**:
    ```bash
    dotnet test
    ```
    You should see a summary of passing tests.

## 5. Next Steps & Future Improvements

This project is a starting point. A great way to continue learning would be to implement the following:

*   **Add a Real Database**: Replace the `DummyGameRepository` with a new repository that uses **Entity Framework Core** to connect to a database like PostgreSQL or SQL Server.
*   **Implement Logging**: Add a logging library like Serilog to record important events and errors.
*   **Add Validation**: Use a library like FluentValidation to validate incoming DTOs.
*   **Expand the Domain**: Add more properties to the `Game` class, such as `Genre`, `ReleaseYear`, or a user rating.
