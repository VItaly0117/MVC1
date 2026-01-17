using System.ComponentModel.DataAnnotations;

namespace MVC.Models.Forms;

public class CategoryForm
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}