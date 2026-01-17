using System.ComponentModel.DataAnnotations;

namespace MVC.Models.Forms;

public class UserProfileForm
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    public IFormFile? Avatar { get; set; }
}