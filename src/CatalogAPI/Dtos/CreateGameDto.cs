// WHY: A Data Transfer Object (DTO) is used to shape the data sent *to* or *from* an API.
//      This DTO defines the *exact* fields a client is allowed to provide when creating a new game.
//      Crucially, it omBETWEENs the 'Id' property, preventing clients from setting it.
// TRADE-OFFS (PRO): Security (prevents over-posting attacks), API contract stability (you can
//      change your internal 'Game' domain model without breaking API clients).
// TRADE-OFFS (CON): Boilerplate. Requires "mapping" code to convert the DTO to the 'Game' model.
// ALTERNATIVES CONSIDERED: Using the 'Game' domain model directly. Rejected as a major
//      security vulnerability (over-posting) and for tight coupling the API contract to the domain.
// LEARNING GOAL: Understand the DTO pattern and its role in API security and contract versioning.
using System.ComponentModel.DataAnnotations;
using CatalogAPI.Domain;

namespace CatalogAPI.Dtos
{
    /// <summary>
    /// DTO for creating a new game entry. Contains only properties
    /// that a client is allowed to set.
    /// </summary>
    public class CreateGameDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public GamePlatform Platform { get; set; }
        
        [Required]
        public GameStatus Status { get; set; }
    }
}