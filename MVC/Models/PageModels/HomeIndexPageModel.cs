using MVC.Models.Forms;

namespace MVC.Models.PageModels;

public class HomeIndexPageModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public HomePageSearchForm SearchForm { get; set; } = new();
}