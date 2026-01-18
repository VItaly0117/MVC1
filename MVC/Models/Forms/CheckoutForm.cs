using System.ComponentModel.DataAnnotations;

namespace MVC.Models.Forms;

public class CheckoutForm
{
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Address Line 1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [Display(Name = "Address Line 2 (Optional)")]
    public string? AddressLine2 { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.PostalCode)]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = string.Empty;
}