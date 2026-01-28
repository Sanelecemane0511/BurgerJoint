using System.ComponentModel.DataAnnotations;
namespace BurgerJoint.Models
{
public class Burger
{
public int Id { get; set; }
 [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1000.00)]
    public decimal Price { get; set; }

    [Required, StringLength(300)]
    public string Ingredients { get; set; } = string.Empty;

    [Display(Name = "Image File Name")]
    public string? ImageFileName { get; set; } = string.Empty;
}
}