using System;
using System.ComponentModel.DataAnnotations;

namespace CatalogAPI.Domain;

public class Game
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Titile { get; set; } = string.Empty;
    public GamePlatform Platform { get; set; }
    public GameStatus Status { get; set; }
}
