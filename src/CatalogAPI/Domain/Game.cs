using System;

namespace CatalogAPI.Domain;
/// <summary>
/// Represents a single game in the user's backlog.
///
/// WHY: This is our core Domain Model (or "Entity"). It represents the "thing"
/// our application is all about. It's a simple POCO (Plain Old C# Object)
/// with no dependencies on databases, APIs, or any other framework.
/// This adheres to the Single Responsibility Principle (SRP) - its only
/// responsibility is to hold the state of a game.
///
/// LEARNING GOAL: Understand how to define clean domain models that are
/// persistence-ignorant. This entity knows nothing about EF Core or SQL Server.
/// </summary>
public class Game
{
    public int Id { get; set; } 
    public required string Title { get; set; }   
    public string? Developer { get; set; } 
    public string? Genre { get; set; }  
    public DateTime DateAdded { get; set; }
}
