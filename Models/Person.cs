using System.ComponentModel.DataAnnotations;

namespace PerryHomesTracker.Models;

public class Person
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(256)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(40)]
    [Phone]
    public string? Phone { get; set; }

    [MaxLength(120)]
    [Display(Name = "Role")]
    public string? Role { get; set; }

    public ICollection<Home> HomesAsPrimaryContact { get; set; } = new List<Home>();
}
