using System.ComponentModel.DataAnnotations;

namespace MVC.Models.Forms;

public class ProductForm
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    public string? Description { get; set; }

    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public ICollection<IFormFile> Images { get; set; } = new List<IFormFile>();

    public ProductTagsForm TagsForm { get; set; } = new();
}

public class ProductTagsForm
{
    public List<ProductTagsFormItem> Items { get; set; } = new();
}

public class ProductTagsFormItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}