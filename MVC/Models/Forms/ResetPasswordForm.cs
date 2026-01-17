using System.ComponentModel.DataAnnotations;

namespace MVC.Models.Forms;

public class ResetPasswordForm
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;
}