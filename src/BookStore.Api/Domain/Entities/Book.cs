using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Api.Domain.Entities;

public class Book
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [Required]
    [StringLength(13)]
    public string Isbn { get; set; } = string.Empty;

    [Required]
    public int Year { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Required]
    public int Stock { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
