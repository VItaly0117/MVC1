﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models;

[Table("Tag")]
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}