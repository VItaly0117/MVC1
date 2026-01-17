﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models;

public class ImageUploaded
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;

    [NotMapped]
    public string Src
    {
        get
        {
            return $"/uploads/images/{FileName[0]}/{FileName[1]}/{FileName}";
        }
    }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}