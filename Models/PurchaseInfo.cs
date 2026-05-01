using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerryHomesTracker.Models;

public class PurchaseInfo
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Home")]
    public int HomeId { get; set; }

    public Home Home { get; set; } = null!;

    [DataType(DataType.Date)]
    [Display(Name = "Contract date")]
    public DateTime? ContractDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Closing date")]
    public DateTime? ClosingDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Purchase price")]
    [Range(0, double.MaxValue)]
    public decimal? PurchasePrice { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
