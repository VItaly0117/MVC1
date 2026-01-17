﻿namespace MVC.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int? ImageUploadedId { get; set; }
    public ImageUploaded? Image { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}