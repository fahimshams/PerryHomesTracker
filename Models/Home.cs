using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerryHomesTracker.Models;

public class Home
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Display(Name = "Address line 1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? AddressLine2 { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(2)]
    public string State { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Display(Name = "ZIP")]
    public string Zip { get; set; } = string.Empty;

    [MaxLength(150)]
    [Display(Name = "Community")]
    public string? CommunityName { get; set; }

    [MaxLength(120)]
    [Display(Name = "Plan")]
    public string? PlanName { get; set; }

    [Display(Name = "Stage")]
    public int StageId { get; set; }

    public Stage Stage { get; set; } = null!;

    [Display(Name = "Primary contact")]
    public int? PrimaryContactId { get; set; }

    public Person? PrimaryContact { get; set; }

    public PurchaseInfo? PurchaseInfo { get; set; }

    [NotMapped]
    [Display(Name = "Full address")]
    public string FullAddress =>
        string.IsNullOrEmpty(AddressLine2)
            ? $"{AddressLine1}, {City}, {State} {Zip}"
            : $"{AddressLine1}, {AddressLine2}, {City}, {State} {Zip}";
}
