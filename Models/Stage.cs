using System.ComponentModel.DataAnnotations;

namespace PerryHomesTracker.Models;

public class Stage
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Sort order")]
    public int SortOrder { get; set; }

    public ICollection<Home> Homes { get; set; } = new List<Home>();
}
